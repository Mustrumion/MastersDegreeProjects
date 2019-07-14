using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using InstanceSolvers.Solvers.Base;
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
        public int NumberOfAcceptableSolutions { get; set; }
        public string RepositoryVersionHash { get; set; }
        public TimeSpan TotalTime { get; set; }
        public TasksStats TasksStats { get; set; } = new TasksStats();
        public ISolver Solver { get; set; }
    }
}
