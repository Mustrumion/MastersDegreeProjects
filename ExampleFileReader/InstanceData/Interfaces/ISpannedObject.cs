using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleFileReader.InstanceData.Interfaces
{
    public interface ISpannedObject
    {
        TimeSpan Span { get; set; }
        int SpanUnits { get; set; }
        DateTime StartTime { get; set; }
        DateTime EndTime { get; set; }
    }
}
