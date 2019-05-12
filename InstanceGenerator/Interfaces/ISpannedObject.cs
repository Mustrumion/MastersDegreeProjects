using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGenerator.Interfaces
{
    public interface ISpannedObject
    {
        int SpanUnits { get; set; }
        TimeSpan Span { get; set; }
        DateTime StartTime { get; set; }
        DateTime EndTime { get; set; }
    }
}
