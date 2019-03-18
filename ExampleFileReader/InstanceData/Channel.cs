using ExampleFileReader.InstanceData.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleFileReader.InstanceData
{
    public class Channel
    {
        public string ID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public List<BaseActivity> Activities { get; set; } = new List<BaseActivity>();
        public List<Autopromotion> Autopromotions { get; set; } = new List<Autopromotion>();
        public List<TvProgram> Programs { get; set; } = new List<TvProgram>();
        public List<Advertisement> Advertisements { get; set; } = new List<Advertisement>();
    }
}
