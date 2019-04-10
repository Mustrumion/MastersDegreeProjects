using ExampleFileReader.InstanceData.Activities;
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


        [XmlIgnore]
        public Instance Instance { get; set; }
        [XmlIgnore]
        public List<BaseActivity> Activities { get; set; } = new List<BaseActivity>();
        [XmlIgnore]
        public List<Autopromotion> Autopromotions { get; set; } = new List<Autopromotion>();
        [XmlIgnore]
        public List<TvProgram> Programs { get; set; } = new List<TvProgram>();
        [XmlIgnore]
        public List<Advertisement> Advertisements { get; set; } = new List<Advertisement>();
        public List<TvBreak> Breaks { get; set; } = new List<TvBreak>();

        public void AddBreak(TvBreak @break)
        {
            Breaks.Add(@break);
        }
    }
}
