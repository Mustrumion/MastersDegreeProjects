using InstanceSolvers;
using InstanceSolvers.Solvers;
using InstanceSolvers.Solvers.Base;
using InstanceSolvers.TransformationFactories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGeneratorConsole
{
    public class RandomAlgorithmsExperiment
    {
        private static string MAIN_DIRECTORY = @"C:\Users\mustrum\dropbox\MDP";

        public void Perform()
        {
            BulkSolver bulkSolver = new BulkSolver()
            {
                MainDirectory = MAIN_DIRECTORY,
                ParallelExecution = true,
                MaxThreads = 15,
                LengthFilter = new[] { "week.json", "month.json" },
            };
            bulkSolver.SolveEverything(RandomSlow);
            bulkSolver.SolveEverything(RandomFast);
        }


        private ISolver RandomSlow()
        {
            RandomInsertsSolver randomSolver = new RandomInsertsSolver()
            {
                Description = "experiment_random_slow",
                DiagnosticMessages = true,
                ScoringFunction = new Scorer(),
                TimeLimit = new TimeSpan(0, 20, 0),
            };
            return randomSolver;
        }
        

        private ISolver RandomFast()
        {
            RandomFastSolver randomSolver = new RandomFastSolver()
            {
                Description = "experiment_random_fast",
                DiagnosticMessages = true,
                ScoringFunction = new Scorer(),
            };
            
            return randomSolver;
        }
    }
}
