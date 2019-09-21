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
                MainDirectory = MAIN_DIRECTORY,
                ParallelExecution = true,
                ReportProgrssToFile = true,
                MaxThreads = 15,
                LengthFilter = new[] { "week.json", "month.json" },
            };
            bulkSolver.SolveEverything(LocalSearchFinal);
            bulkSolver.SolveEverything(LocalSearchFinal2);
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
                Description = "base_popualtion_ls_compat_oneforall",
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
