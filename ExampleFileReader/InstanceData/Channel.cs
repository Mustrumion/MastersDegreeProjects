using ExampleFileReader.InstanceData.Activities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ExampleFileReader.InstanceData
{
    [Serializable]
    public class Channel
    {
        public string ID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        
        public Instance Instance { get; set; }
        public List<Autopromotion> Autopromotions { get; set; } = new List<Autopromotion>();
        public List<TvProgram> Programs { get; set; } = new List<TvProgram>();
        public List<AdvertisementInstance> Advertisements { get; set; } = new List<AdvertisementInstance>();
        public List<TvBreak> Breaks { get; set; } = new List<TvBreak>();
        [JsonProperty(Order = 1)]
        public List<BaseActivity> Activities { get; set; } = new List<BaseActivity>();

        public void AddBreak(TvBreak @break)
        {
            Breaks.Add(@break);
        }
    }
}
