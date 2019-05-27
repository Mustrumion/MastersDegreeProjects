using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGenerator.SolutionObjects
{
    public class Solution
    {
        private Dictionary<int, List<int>> _advertisementsScheduledOnBreaks = new Dictionary<int, List<int>>();

        /// <summary>
        /// Dictionary of where in the solution are instances of AdOrders. Outer key - orderID. Inner key - break ID, inner value - ad position
        /// </summary>
        [JsonIgnore]
        public Dictionary<int, TaskData> AdOrderInstances { get; set; } = new Dictionary<int, TaskData>();

        [JsonIgnore]
        public Instance Instance { get; set; }

        [JsonIgnore]
        public IScoringFunction GradingFunction { get; set; }

        [Description("Overall solution score.")]
        public double WeightedLoss { get; set; }

        [Description("Loss from late ad contract completion.")]
        public double OverdueAdsLoss { get; set; }

        [Description("Loss form scheduling soft-incompatible ads on the same break.")]
        public double MildIncompatibilityLoss { get; set; }

        [Description("Loss form overextending breaks.")]
        public double ExtendedBreakLoss { get; set; }

        [Description("Number of advertisement orders (tasks) with hard constraints met.")]
        public int Integrity { get; set; }
        
        [JsonProperty(Order = 1)]
        [Description("Dictionary of lists. Dictionary keys represent break IDs. Lists contain job IDs in order scheduled for a break given by the key.")]
        public Dictionary<int, List<int>> AdvertisementsScheduledOnBreaks
        {
            get => _advertisementsScheduledOnBreaks;
            set
            {
                _advertisementsScheduledOnBreaks = value;
                RestoreHelperStructures();
            }
        }

        [Description("Fraction of tasks with hard constraints met.")]
        public double IntegrityScore
        {
            get
            {
                return (double)Integrity / MaxIntegrity;
            }
            set { }
        }

        [Description("Number of tasks.")]
        public int MaxIntegrity
        {
            get
            {
                return Instance.AdOrders.Count;
            }
            set { }
        }

        public string GradingFunctionDescription
        {
            get
            {
                return GradingFunction?.Description;
            }
            set { }
        }


        public void GenerateSolutionFromRealData()
        {
            AdvertisementsScheduledOnBreaks = new Dictionary<int, List<int>>();
            foreach (var tvBreak in Instance.Channels.Values.SelectMany(c => c.Breaks))
            {
                AdvertisementsScheduledOnBreaks.Add(tvBreak.ID, tvBreak.Advertisements.Select(a => a.AdOrderID).ToList());
            }
        }


        public void RestoreHelperStructures()
        {
            AdOrderInstances = new Dictionary<int, TaskData>();
            foreach (var tvBreak in AdvertisementsScheduledOnBreaks)
            {
                for (int i = 0; i < tvBreak.Value.Count; i++)
                {
                    AddAdToTaskDataDictionry(tvBreak.Value[i], tvBreak.Key, i);
                }
            }
        }


        /// <summary>
        /// Adds an advertisement instance to the break in the task data helper structure.
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="breakId"></param>
        /// <param name="position"></param>
        private void AddAdToTaskDataDictionry(int orderId, int breakId, int position)
        {
            bool success = AdOrderInstances.TryGetValue(orderId, out var taskData);
            if (!success)
            {
                taskData = new TaskData() { TaskID = orderId, AdvertisementOrderData = Instance.AdOrders[orderId] };
                AdOrderInstances.Add(orderId, taskData);
            }
            success = taskData.BreaksPositions.TryGetValue(breakId, out var breakPositions);
            if (!success)
            {
                breakPositions = new List<int>();
                taskData.BreaksPositions.Add(breakId, breakPositions);
            }
            breakPositions.Add(position);
        }


        /// <summary>
        /// Removes an advertisement instance from the break in the task data helper structure.
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="breakId"></param>
        /// <param name="position"></param>
        private void RemoveAdFromTaskDataDictionary(int orderId, int breakId, int position)
        {
            var taskData = AdOrderInstances[orderId];
            var breakPositions = taskData.BreaksPositions[breakId];
            breakPositions.Remove(position);
            if (breakPositions.Count == 0)
            {
                taskData.BreaksPositions.Remove(breakId);
            }
            if (taskData.BreaksPositions.Count == 0)
            {
                AdOrderInstances.Remove(orderId);
            }
        }


        /// <summary>
        /// Adds an advertisement instance to the solution.
        /// </summary>
        /// <param name="ad"></param>
        /// <param name="tvBreak"></param>
        /// <param name="position"></param>
        public void AddAdToBreak(AdvertisementOrder ad, TvBreak tvBreak, int position)
        {
            List<int> adsInBreak = _advertisementsScheduledOnBreaks[tvBreak.ID];
            //Move positions by one in helper structure.
            for (int i = position; i < adsInBreak.Count; i++)
            {
                int adId = adsInBreak[i];
                var positionsInBreak = AdOrderInstances[adId].BreaksPositions[tvBreak.ID];
                for (int j = 0; j < positionsInBreak.Count; i++)
                {
                    if (positionsInBreak[j] >= position)
                    {
                        positionsInBreak[j] += 1;
                    }
                }
            }
            AddAdToTaskDataDictionry(ad.ID, tvBreak.ID, position);
            _advertisementsScheduledOnBreaks[tvBreak.ID].Insert(position, ad.ID);
        }

        /// <summary>
        /// Removes an advertisement instance from the solution.
        /// </summary>
        /// <param name="ad"></param>
        /// <param name="tvBreak"></param>
        /// <param name="position"></param>
        public void RemoveAdFromBreak(AdvertisementOrder ad, TvBreak tvBreak, int position)
        {
            List<int> adsInBreak = _advertisementsScheduledOnBreaks[tvBreak.ID];
            //Move positions by one in helper structure.
            for (int i = position + 1; i < adsInBreak.Count; i++)
            {
                int adId = adsInBreak[i];
                var positionsInBreak = AdOrderInstances[adId].BreaksPositions[tvBreak.ID];
                for (int j = 0; j < positionsInBreak.Count; i++)
                {
                    if (positionsInBreak[j] >= position)
                    {
                        positionsInBreak[j] -= 1;
                    }
                }
            }
            RemoveAdFromTaskDataDictionary(ad.ID, tvBreak.ID, position);
            _advertisementsScheduledOnBreaks[tvBreak.ID].RemoveAt(position);
        }
    }
}
