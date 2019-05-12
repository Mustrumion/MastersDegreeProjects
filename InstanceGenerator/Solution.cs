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
        [JsonProperty(Order = 1)]
        [Description("Dictionary of lists. Dictionary keys represent break IDs. Lists contain job IDs in order scheduled for a break given by the key.")]
        public Dictionary<String, List<string>> AdvertisementsScheduledOnBreaks { get; set; } = new Dictionary<string, List<string>>();

        [JsonIgnore]
        public Dictionary<String, Dictionary<string, int>> AdOrderInstances { get; set; } = new Dictionary<string, Dictionary<string, int>>();

        [JsonIgnore]
        public Instance Instance { get; set; }

        [JsonIgnore]
        public IScoringFunction GradingFunction { get; set; }

        [Description("Objective function score.")]
        public double Score { get; set; }

        [Description("Number of advertisement orders (tasks) with hard constraints met.")]
        public int Integrity { get; set; }



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
            AdvertisementsScheduledOnBreaks = new Dictionary<string, List<string>>();
            foreach (var tvBreak in Instance.Channels.Values.SelectMany(c => c.Breaks))
            {
                AdvertisementsScheduledOnBreaks.Add(tvBreak.ID, tvBreak.Advertisements.Select(a => a.AdOrderID).ToList());
            }
        }
    }
}
