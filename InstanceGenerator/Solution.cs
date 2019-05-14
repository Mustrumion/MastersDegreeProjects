using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGenerator
{
    public class Solution
    {
        private Dictionary<int, List<int>> _advertisementsScheduledOnBreaks = new Dictionary<int, List<int>>();

        /// <summary>
        /// Dictionary of where in the solution are instances of AdOrders. Outer key - orderID. Inner key - break ID, inner value - ad position
        /// </summary>
        [JsonIgnore]
        public Dictionary<int, Dictionary<int, List<int>>> AdOrderInstances { get; set; } = new Dictionary<int, Dictionary<int, List<int>>>();

        [JsonIgnore]
        public Instance Instance { get; set; }

        [JsonIgnore]
        public IScoringFunction GradingFunction { get; set; }

        [Description("Objective function score.")]
        public double Score { get; set; }

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
            AdOrderInstances = new Dictionary<int, Dictionary<int, List<int>>>();
            foreach (var tvBreak in AdvertisementsScheduledOnBreaks)
            {
                for (int i = 0; i < tvBreak.Value.Count; i++)
                {
                    AddAdPositionToOrdersDictionry(tvBreak.Value[i], tvBreak.Key, i);
                }
            }
        }


        private void AddAdPositionToOrdersDictionry(int orderId, int breakId, int position)
        {
            bool success = AdOrderInstances.TryGetValue(orderId, out var breaksLists);
            if (!success)
            {
                breaksLists = new Dictionary<int, List<int>>();
                AdOrderInstances.Add(orderId, breaksLists);
            }
            success = breaksLists.TryGetValue(breakId, out var breakPositions);
            if (!success)
            {
                breakPositions = new List<int>();
                breaksLists.Add(breakId, breakPositions);
            }
            breakPositions.Add(position);
        }
    }
}
