using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers
{
    public class ReportEntry
    {
        public DateTime Time { get; set; }
        public double SecondsFromStart { get; set; }
        public double IntegrityLoss { get; set; }
        public double WeightedLoss { get; set; }
        public bool AttainedAcceptable { get; set; }
        public string Action { get; set; }
    }
}
