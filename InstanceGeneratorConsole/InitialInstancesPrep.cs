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
    public class InitialInstancesPrep
    {
        private static string MAIN_DIRECTORY = @"C:\Users\mustrum\dropbox\MDP";

        public void Perform()
        {
            BulkSolver bulkSolver = new BulkSolver()
            {
                Times = 38,
                MainDirectory = MAIN_DIRECTORY,
                ParallelExecution = true,
                ReportProgrssToFile = true,
                MaxThreads = 15,
                DifficultyFilter = new[] { "trivial", "easy", "very_easy", "hard", "extreme" },
                LengthFilter = new[] { "week.json", "month.json" },
            };
            bulkSolver.SolveEverything(LocalSearchFinal);
        }


        private ISolver LocalSearchFinal()
        {
            GreedyFastHeuristic randomSolver = new GreedyFastHeuristic()
            {
            };
            CompoundSolver compundSolver = new CompoundSolver()
            {
                MaxLoops = 5,
                TimeLimit = new TimeSpan(0, 3, 0),
            };
            LocalSearch solver = new LocalSearch()
            {
                ScoringFunction = new Scorer(),
                DiagnosticMessages = true,
                PropagateRandomSeed = true,
                NumberOfNoGoodActionsToStop = 20,
                BestFactoryAdjustmentParam = 0.2,
                NeighberhoodAdjustmentParam = 0.2,
                ImprovementOverNarrowNeighb = 2,
                TimeLimit = new TimeSpan(0, 6, 0),
                Description = "base_pop_38",
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


        private ISolver LocalSearchFinal2()
        {
            RandomFastSolver randomSolver = new RandomFastSolver()
            {
            };
            CompoundSolver compundSolver = new CompoundSolver()
            {
                MaxLoops = 5,
                TimeLimit = new TimeSpan(0, 3, 0),
            };
            LocalSearch solver = new LocalSearch()
            {
                ScoringFunction = new Scorer(),
                DiagnosticMessages = true,
                PropagateRandomSeed = true,
                NumberOfNoGoodActionsToStop = 20,
                BestFactoryAdjustmentParam = 0.2,
                NeighberhoodAdjustmentParam = 0.2,
                ImprovementOverNarrowNeighb = 2,
                TimeLimit = new TimeSpan(0, 6, 0),
                Description = "base_popualtion_ls_random_oneforall",
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
