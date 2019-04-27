using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleFileReader.InstanceData
{
    public class ViewershipSpan
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public TimeSpan Span { get; set; }
        public double Viewers { get; set; }
    }
}
