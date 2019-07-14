using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers
{
    public interface IReporter
    {
        void Start();
        void AddEntry(ReportEntry entry);
        void Save(string path);
    }
}
