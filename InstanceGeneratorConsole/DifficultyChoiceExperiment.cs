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
    public class DifficultyChoiceExperiment
    {
        private static string MAIN_DIRECTORY = @"C:\Users\bartl_000\dropbox\MDP";

        public void Perform()
        {
            BulkSolver bulkSolver = new BulkSolver()
            {
                MainDirectory = MAIN_DIRECTORY,
                ParallelExecution = false,
                MaxThreads = 4,
                TotalStatsCategories = new[] { "trivial", "very_easy", "easy", "medium", "hard", "extreme" },
                LengthFilter = new[] { "week.json", "month" },
            };
            bulkSolver.SolveEverything(LocalSearchFinal);
        }


        private ISolver LocalSearchFinal()
        {
            GreedyFastHeuristic randomSolver = new GreedyFastHeuristic()
            {
                MaxOverfillUnits = 0,
                DiagnosticMessages = true,
            };
            CompoundSolver compundSolver = new CompoundSolver()
            {
                MaxLoops = 7,
                DiagnosticMessages = true,
            };
            LocalSearch solver = new LocalSearch()
            {
                ScoringFunction = new Scorer(),
                DiagnosticMessages = true,
                PropagateRandomSeed = true,
                NumberOfNoGoodActionsToStop = 15,
                BestFactoryAdjustmentParam = 0.2,
                NeighberhoodAdjustmentParam = 0.2,
                ImprovementOverNarrowNeighb = 2,
                TimeLimit = new TimeSpan(0, 5, 0),
                Description = "local_search_final_difficulty_test",
            };
            solver.MoveFactories = new List<ITransformationFactory>
            {
                new InsertFactory()
                {
                    MildlyRandomOrder = true,
                    PositionsCountLimit = 4,
                    MaxTasksChecked = 3,
                    MaxBreaksChecked = 3,
                    IgnoreBreaksWhenUnitOverfillAbove = 60,
                    IgnoreCompletedTasks = true,
                    IgnoreTasksWithCompletedViews = false,
                    AlwaysReturnStartsAndEnds = true,
                },
                new RandomDeleteFactory()
                {
                    MovesReturned = 20,
                },
                new RandomInsertFactory()
                {
                    MovesReturned = 30,
                },
                new RandomSwapFactory()
                {
                    MovesReturned = 30,
                },
            };
            solver.InitialSolvers.Add(randomSolver);
            solver.InitialSolvers.Add(compundSolver);
            return solver;
        }
    }
}
