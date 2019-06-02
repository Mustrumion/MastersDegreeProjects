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
        private Dictionary<int, List<AdvertisementOrder>> _advertisementsScheduledOnBreaks = new Dictionary<int, List<AdvertisementOrder>>();
        private IScoringFunction _gradingFunction;

        [JsonIgnore]
        public Instance Instance { get; set; }

        [JsonIgnore]
        public IScoringFunction GradingFunction
        {
            get => _gradingFunction;
            set
            {
                _gradingFunction = value;
                _gradingFunction.Instance = Instance;
                _gradingFunction.Solution = this;
            }
        }

        /// <summary>
        /// Dictionary of lists. Dictionary keys represent break IDs. Lists contain job objects in order scheduled for a break given by the key.
        /// </summary>
        [JsonIgnore]
        public Dictionary<int, List<AdvertisementOrder>> AdvertisementsScheduledOnBreaks
        {
            get => _advertisementsScheduledOnBreaks;
            set
            {
                _advertisementsScheduledOnBreaks = value;
                RestoreTaskView();
            }
        }

        /// <summary>
        /// Overall solution score.
        /// </summary>
        [Description("Overall solution score.")]
        public double WeightedLoss { get; set; }

        /// <summary>
        /// Loss from late ad contract completion.
        /// </summary>
        [Description("Loss from late ad contract completion.")]
        public double OverdueAdsLoss { get; set; }

        /// <summary>
        /// Loss form scheduling soft-incompatible ads on the same break.
        /// </summary>
        [Description("Loss form scheduling soft-incompatible ads on the same break.")]
        public double MildIncompatibilityLoss { get; set; }

        /// <summary>
        /// Loss form overextending breaks.
        /// </summary>
        [Description("Loss form overextending breaks.")]
        public double ExtendedBreakLoss { get; set; }

        /// <summary>
        /// Detailed integrity score. Subjective, depends on grading object.
        /// </summary>
        [Description("Detailed integrity score.")]
        public double IntegrityLossScore { get; set; }

        /// <summary>
        /// Number of advertisement orders (tasks) with hard constraints met.
        /// </summary>
        [Description("Number of advertisement orders (tasks) with hard constraints met.")]
        public int Completion { get; set; }

        /// <summary>
        /// Dictionary of task stats and where in the solution are instances of the tasks. Key - ID of the order (task).
        /// </summary>
        [Description("Dictionary of task stats and where in the solution are instances of the tasks. Key - ID of the order (task).")]
        [JsonProperty(Order = 1)]
        public Dictionary<int, TaskData> AdOrderData { get; set; } = new Dictionary<int, TaskData>();

        /// <summary>
        /// View on the current solution state. Created for serialization purpose. 
        /// Dictionary of lists. Dictionary keys represent break IDs. Lists contain job IDs in order scheduled for a break given by the key.
        /// </summary>
        [JsonProperty(Order = 2)]
        [Description("Dictionary of lists. Dictionary keys represent break IDs. Lists contain job IDs in order scheduled for a break given by the key.")]
        public Dictionary<int, List<int>> AdvertisementIdsScheduledOnBreaks { get; set; } = new Dictionary<int, List<int>>();

        /// <summary>
        /// Fraction of tasks with hard constraints met.
        /// </summary>
        [Description("Fraction of tasks with hard constraints met.")]
        public double CompletionScore
        {
            get
            {
                return (double)Completion / MaxCompletion;
            }
            set { }
        }

        /// <summary>
        /// Number of tasks.
        /// </summary>
        [Description("Number of tasks.")]
        public int MaxCompletion
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
            _advertisementsScheduledOnBreaks = new Dictionary<int, List<AdvertisementOrder>>();
            foreach (var tvBreak in Instance.Channels.Values.SelectMany(c => c.Breaks))
            {
                _advertisementsScheduledOnBreaks.Add(tvBreak.ID, tvBreak.Advertisements.Select(a => a.AdvertisementOrder).ToList());
            }
        }

        public void PrepareForSerialization()
        {
            AdvertisementIdsScheduledOnBreaks = _advertisementsScheduledOnBreaks.ToDictionary(
                b => b.Key,
                b => b.Value.Select(a => a.ID).ToList());
        }

        public void RestoreStructures()
        {
            _advertisementsScheduledOnBreaks = AdvertisementIdsScheduledOnBreaks.ToDictionary(
                b => b.Key,
                b => b.Value.Select(a => Instance.AdOrders[a]).ToList());
            RestoreTaskView();
        }

        public void RestoreTaskView()
        {
            AdOrderData = new Dictionary<int, TaskData>();
            foreach (var tvBreak in _advertisementsScheduledOnBreaks)
            {
                for (int i = 0; i < tvBreak.Value.Count; i++)
                {
                    AddAdToTaskDataDictionry(tvBreak.Value[i].ID, tvBreak.Key, i);
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
            bool success = AdOrderData.TryGetValue(orderId, out var taskData);
            if (!success)
            {
                taskData = new TaskData() { AdvertisementOrderData = Instance.AdOrders[orderId] };
                AdOrderData.Add(orderId, taskData);
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
            var taskData = AdOrderData[orderId];
            var breakPositions = taskData.BreaksPositions[breakId];
            breakPositions.Remove(position);
            if (breakPositions.Count == 0)
            {
                taskData.BreaksPositions.Remove(breakId);
            }
            if (taskData.BreaksPositions.Count == 0)
            {
                AdOrderData.Remove(orderId);
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
            var adsInBreak = _advertisementsScheduledOnBreaks[tvBreak.ID];
            //Move positions by one in helper structure.
            for (int i = position; i < adsInBreak.Count; i++)
            {
                int adId = adsInBreak[i].ID;
                var positionsInBreak = AdOrderData[adId].BreaksPositions[tvBreak.ID];
                for (int j = 0; j < positionsInBreak.Count; i++)
                {
                    if (positionsInBreak[j] >= position)
                    {
                        positionsInBreak[j] += 1;
                    }
                }
            }
            AddAdToTaskDataDictionry(ad.ID, tvBreak.ID, position);
            _advertisementsScheduledOnBreaks[tvBreak.ID].Insert(position, ad);
        }

        /// <summary>
        /// Removes an advertisement instance from the solution.
        /// </summary>
        /// <param name="ad"></param>
        /// <param name="tvBreak"></param>
        /// <param name="position"></param>
        public void RemoveAdFromBreak(AdvertisementOrder ad, TvBreak tvBreak, int position)
        {
            var adsInBreak = _advertisementsScheduledOnBreaks[tvBreak.ID];
            //Move positions by one in helper structure.
            for (int i = position + 1; i < adsInBreak.Count; i++)
            {
                int adId = adsInBreak[i].ID;
                var positionsInBreak = AdOrderData[adId].BreaksPositions[tvBreak.ID];
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
