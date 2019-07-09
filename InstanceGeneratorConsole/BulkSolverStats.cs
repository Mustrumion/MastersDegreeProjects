using InstanceGenerator.SolutionObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGeneratorConsole
{
    public class BulkSolverStats
    {
        public int NumberOfExamples { get; set; }
        public TimeSpan TotalTime { get; set; }
        public TasksStats TasksStats { get; set; } = new TasksStats();
    }
}
