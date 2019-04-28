using ExampleFileReader.InstanceData.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleFileReader.InstanceData
{
    public class ViewershipSpan : ISpannedObject
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Span { get; set; }
        public int SpanUnits { get; set; }
        public double Viewers { get; set; }
    }
}
