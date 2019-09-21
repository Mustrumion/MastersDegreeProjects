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
    public class CompoundVsSeriesExperiment
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
            };
            bulkSolver.SolveEverything(InsertionStartEndingDeleteConfiguration);
            bulkSolver.SolveEverything(CompundNoIncom);
        }

        private static ISolver InsertionStartEndingDeleteConfiguration()
        {
            GreedyFastHeuristic randomSolver = new GreedyFastHeuristic();
            ViewsHeuristic insertionHeuristic = new ViewsHeuristic()
            {
                TimeLimit = new TimeSpan(0, 3, 0),
                MaxBreakExtensionUnits = 40,
                MaxLoops = 6,
                MaxInsertedPerBreak = 5,
            };
            BeginingsHeuristic beginingsHeuristic = new BeginingsHeuristic()
            {
                MaxLoops = 6,
                TimeLimit = new TimeSpan(0, 3, 0),
                MaxBreakExtensionUnits = 999,
            };
            EndingsHeuristic endingHeuristic = new EndingsHeuristic()
            {
                MaxLoops = 6,
                TimeLimit = new TimeSpan(0, 3, 0),
                MaxBreakExtensionUnits = 999,
            };
            FreeSpaceHeuristic freeSpaceHeuristic = new FreeSpaceHeuristic()
            {
                ScoringFunction = new Scorer(),
                TimeLimit = new TimeSpan(0, 3, 0),
                MaxLoops = 6,
                PropagateRandomSeed = true,
                Seed = 10,
                DiagnosticMessages = true,
                Description = "views_starts_ends_trim_heuristic_6loops",
            };
            freeSpaceHeuristic.InitialSolvers.Add(randomSolver);
            freeSpaceHeuristic.InitialSolvers.Add(insertionHeuristic);
            freeSpaceHeuristic.InitialSolvers.Add(beginingsHeuristic);
            freeSpaceHeuristic.InitialSolvers.Add(endingHeuristic);
            return freeSpaceHeuristic;
        }

        private ISolver CompundNoIncom()
        {
            GreedyFastHeuristic randomSolver = new GreedyFastHeuristic()
            {
            };
            CompoundSolver compundSolver = new CompoundSolver()
            {
                ScoringFunction = new Scorer(),
                MaxLoops = 6,
                PropagateRandomSeed = true,
                TimeLimit = new TimeSpan(0, 12, 0),
                Description = "compund_6loops",
            };
            compundSolver.InitialSolvers.Add(randomSolver);
            return compundSolver;
        }
    }
}
