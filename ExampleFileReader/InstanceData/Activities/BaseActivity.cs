using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleFileReader.InstanceData.Activities
{
    public class BaseActivity
    {
        public TimeSpan Span { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
