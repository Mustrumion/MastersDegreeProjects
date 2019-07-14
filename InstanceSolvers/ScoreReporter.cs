using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers
{
    public class ScoreReporter
    {
        private DateTime _start { get; set; }
        private List<ReportEntry> _entries { get; set; }

        public void AddEntry(ReportEntry entry)
        {
            lock (_entries)
            {
                _entries.Add(entry);
            }
        }

        public void Save(string path)
        {

        }
    }
}
