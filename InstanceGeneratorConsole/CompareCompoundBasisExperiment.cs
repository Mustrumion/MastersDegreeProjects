using InstanceSolvers;
using InstanceSolvers.Solvers;
using InstanceSolvers.Solvers.Base;
using InstanceSolvers.TransformationFactories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGeneratorConsole
{
    public class CompareCompoundBasisExperiment
    {
        private static string MAIN_DIRECTORY = @"C:\Users\mustrum\dropbox\MDP";

        public void Perform()
        {
            BulkSolver bulkSolver = new BulkSolver()
            { 
                MainDirectory = MAIN_DIRECTORY,
                ParallelExecution = true,
                MaxThreads = 15,
                ReportProgrssToFile = true,
                LengthFilter = new[] { "week.json", "month.json" },
                DifficultyFilter = new[] { "medium" },
            };
            bulkSolver.SolveEverything(CompoundRand);
            bulkSolver.SolveEverything(CompundNoIncom);
        }

        private ISolver CompoundRand()
        {
            RandomFastSolver randomSolver = new RandomFastSolver()
            {
            };
            CompoundSolver compundSolver = new CompoundSolver()
            {
                ScoringFunction = new Scorer(),
                MaxLoops = 10,
                PropagateRandomSeed = true,
                TimeLimit = new TimeSpan(0, 15, 0),
                Description = "compund_rand",
            };
            compundSolver.InitialSolvers.Add(randomSolver);
            return compundSolver;
        }

        private ISolver CompundNoIncom()
        {
            GreedyFastHeuristic randomSolver = new GreedyFastHeuristic()
            {
            };
            CompoundSolver compundSolver = new CompoundSolver()
            {
                ScoringFunction = new Scorer(),
                MaxLoops = 10,
                PropagateRandomSeed = true,
                TimeLimit = new TimeSpan(0, 15, 0),
                Description = "compund_no_incom",
            };
            compundSolver.InitialSolvers.Add(randomSolver);
            return compundSolver;
        }
    }
}
