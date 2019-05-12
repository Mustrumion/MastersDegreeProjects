using InstanceGenerator.InstanceData;
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
        [Description("Dictionary of lists. Dictionary keys represent break IDs. Lists contain job IDs in order scheduled for a break given by the key.")]
        public Dictionary<String, List<string>> AdvertisementsScheduledOnBreaks { get; set; } = new Dictionary<string, List<string>>();

        [JsonIgnore]
        public Instance Instance { get; set; }
        

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
