using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleFileReader.InstanceData
{
    public class ViewershipFunction
    {
        public List<ViewershipSpan> TimeIntervals { get; set; } = new List<ViewershipSpan>();
        public double GetViewers(TimeSpan timeFromBreakStart)
        {
            return 0.0d;
        }
    }
}
