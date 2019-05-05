using InstanceGenerator.InstanceData.Activities;
using InstanceGenerator.InstanceData.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace InstanceGenerator.InstanceData
{
    [Serializable]
    public class Channel : IActivitiesSequence
    {
        public string ID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Span { get; set; }
        public int SpanUnits { get; set; }


        [JsonIgnore]
        public Instance Instance { get; set; }
        [JsonIgnore]
        public List<Autopromotion> Autopromotions { get; set; } = new List<Autopromotion>();
        [JsonIgnore]
        public List<TvProgram> Programs { get; set; } = new List<TvProgram>();
        [JsonIgnore]
        public List<AdvertisementInstance> Advertisements { get; set; } = new List<AdvertisementInstance>();

        public List<TvBreak> Breaks { get; set; } = new List<TvBreak>();
        
        [JsonIgnore]
        public List<BaseActivity> Activities { get; set; } = new List<BaseActivity>();

        public void AddBreak(TvBreak @break)
        {
            Breaks.Add(@break);
        }
    }
}
