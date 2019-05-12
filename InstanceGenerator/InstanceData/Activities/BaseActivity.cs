using InstanceGenerator.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGenerator.InstanceData.Activities
{
    public class BaseActivity : ISpannedObject
    {
        [JsonProperty(Order = -100)]
        public int SpanUnits { get; set; }
        [JsonProperty(Order = -52)]
        public TimeSpan Span { get; set; }
        [JsonProperty(Order = -51)]
        public DateTime StartTime { get; set; }
        [JsonProperty(Order = -50)]
        public DateTime EndTime { get; set; }
    }
}
