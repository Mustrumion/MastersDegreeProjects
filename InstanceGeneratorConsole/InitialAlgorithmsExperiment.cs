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
    public class InitialAlgorithmsExperiment
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
            bulkSolver.SolveEverything(InitialBad);
            bulkSolver.SolveEverything(InitialGood);
        }


        private ISolver InitialBad()
        {
            ImprovingInsertsHeuristic improvingInserts = new ImprovingInsertsHeuristic()
            {
                Description = "experiment_initial_old",
                DiagnosticMessages = true,
                ScoringFunction = new Scorer(),
                MaxBreakExtensionUnits = 10,
                TimeLimit = new TimeSpan(0, 4, 0),
            };
            return improvingInserts;
        }
        

        private ISolver InitialGood()
        {
            GreedyFastHeuristic randomSolver = new GreedyFastHeuristic()
            {
                MaxOverfillUnits = 0,
            };
            CompoundSolver compundSolver = new CompoundSolver()
            {
                Description = "experiment_initial_new",
                DiagnosticMessages = true,
                ScoringFunction = new Scorer(),
                TimeLimit = new TimeSpan(0, 4, 0),
            };
            compundSolver.InitialSolvers.Add(randomSolver);
            
            return compundSolver;
        }
    }
}
