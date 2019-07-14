using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers
{
    public class NullReporter : IReporter
    {
        public void AddEntry(ReportEntry entry)
        {
        }

        public void Save(string path)
        {
        }

        public void Start()
        {
        }
    }
}
