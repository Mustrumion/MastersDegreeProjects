using InstanceGenerator;
using InstanceGenerator.DataAccess;
using InstanceGenerator.InstanceData;
using InstanceGenerator.InstanceModification;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using InstanceSolvers;
using InstanceSolvers.MoveFactories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGeneratorConsole
{
    class Program
    {
        private static string MAIN_DIRECTORY = @"C:\Users\Mustrum\Dropbox\MDP";


        static void Main(string[] args)
        {
            //BulkInstanceGenerator bulkInstanceGenerator = new BulkInstanceGenerator()
            //{
            //    MainDirectory = MAIN_DIRECTORY,
            //};
            //bulkInstanceGenerator.GenerateAllInstances();

            BulkSolver bulkSolver = new BulkSolver()
            {
                MainDirectory = MAIN_DIRECTORY,
                ParallelExecution = true,
                MaxThreads = 7,
            };
            bulkSolver.SolveEverything(LocalRandomComplex);

            Console.WriteLine("Press any key.");
            Console.ReadKey();
        }

        private static ISolver GenerateLocalSearchSolverConfiguration2()
        {
            FastGreedyHeuristic randomSolver = new FastGreedyHeuristic()
            {
                MaxOverfillUnits = 30,
            };
            InsertionHeuristic insertionHeuristic = new InsertionHeuristic()
            {
                MaxBreakExtensionUnits = 30,
                MaxInsertedPerBreak = 3,
            };
            LocalSearchSolver solver = new LocalSearchSolver()
            {
                Solution = randomSolver.Solution,
                Seed = 10,
                ScoringFunction = new Scorer(),
                StopWhenCompleted = true,
                PropagateRandomnessSeed = true,
                TimeLimit = new TimeSpan(0, 0, 300),
                Description = "local_random2",
            };
            solver.InitialSolvers.Add(randomSolver);
            solver.InitialSolvers.Add(insertionHeuristic);
            return solver;
        }

        private static ISolver GenerateLocalSearchSolverConfiguration3()
        {
            FastGreedyHeuristic randomSolver = new FastGreedyHeuristic()
            {
                MaxOverfillUnits = -10,
            };
            InsertionHeuristic insertionHeuristic = new InsertionHeuristic()
            {
                MaxBreakExtensionUnits = 30,
                MaxInsertedPerBreak = 3,
                TimeLimit = new TimeSpan(0, 0, 30),
            };
            LocalSearchSolver solver = new LocalSearchSolver()
            {
                Solution = randomSolver.Solution,
                Seed = 10,
                ScoringFunction = new Scorer(),
                StopWhenCompleted = true,
                PropagateRandomnessSeed = true,
                TimeLimit = new TimeSpan(0, 0, 300),
                Description = "local_random3",
            };
            solver.MoveFactories = new List<IMoveFactory>
            {
                new InsertMoveFactory()
                {
                    MildlyRandomOrder = true,
                    PositionsCountLimit = 5,
                    MaxTasksChecked = 4,
                    MaxBreaksChecked = 4,
                    IgnoreBreaksWhenUnitOverfillAbove = 60,
                    IgnoreCompletedTasks = true,
                    IgnoreTasksWithCompletedViews = false,
                },
                new InsertMoveFactory()
                {
                    MildlyRandomOrder = true,
                    PositionsCountLimit = 5,
                    MaxTasksChecked = 4,
                    MaxBreaksChecked = 4,
                    IgnoreBreaksWhenUnitOverfillAbove = 60,
                    IgnoreCompletedTasks = false,
                    IgnoreTasksWithCompletedViews = false,
                },
                new DeleteMoveFactory()
                {
                    MildlyRandomOrder = true,
                    PositionsCountLimit = 4,
                    MaxBreaksChecked = 5,
                },
                new SwapMoveFactory()
                {
                    MildlyRandomOrder = true,
                    PositionsCountLimit = 5,
                    MaxTasksChecked = 5,
                    MaxBreaksChecked = 5,
                },
            };
            solver.InitialSolvers.Add(randomSolver);
            solver.InitialSolvers.Add(insertionHeuristic);
            return solver;
        }

        private static ISolver GenerateInsertionSolverConfiguration()
        {
            FastGreedyHeuristic randomSolver = new FastGreedyHeuristic()
            {
                MaxOverfillUnits = -10,
            };
            InsertionHeuristic insertionHeuristic = new InsertionHeuristic()
            {
                MaxBreakExtensionUnits = 40,
                MaxInsertedPerBreak = 5,
                ScoringFunction = new Scorer(),
                MaxLoops = 5,
                TimeLimit = new TimeSpan(0, 0, 40),
                PropagateRandomSeed = true,
                DiagnosticMessages = true,
                Seed = 10,
                Description = "insertion_heuristic2",
            };
            insertionHeuristic.InitialSolvers.Add(randomSolver);
            return insertionHeuristic;
        }

        private static ISolver GenerateStartInsertionSolverConfiguration()
        {
            FastGreedyHeuristic randomSolver = new FastGreedyHeuristic()
            {
                MaxOverfillUnits = -10,
            };
            InsertionHeuristic insertionHeuristic = new InsertionHeuristic()
            {
                MaxBreakExtensionUnits = 40,
                MaxLoops = 5,
                TimeLimit = new TimeSpan(0, 0, 40),
                MaxInsertedPerBreak = 5,
            };
            BeginingsHeuristic beginingsHeuristic = new BeginingsHeuristic()
            {
                MaxBreakExtensionUnits = 70,
                ScoringFunction = new Scorer(),
                MaxLoops = 3,
                TimeLimit = new TimeSpan(0, 0, 30),
                PropagateRandomSeed = true,
                DiagnosticMessages = true,
                Seed = 10,
                Description = "insertion_starts_heuristic",
            };
            beginingsHeuristic.InitialSolvers.Add(randomSolver);
            beginingsHeuristic.InitialSolvers.Add(insertionHeuristic);
            return beginingsHeuristic;
        }

        private static ISolver GeneratInsertionStartEndingSolverConfiguration()
        {
            FastGreedyHeuristic randomSolver = new FastGreedyHeuristic()
            {
                MaxOverfillUnits = -10,
            };
            InsertionHeuristic insertionHeuristic = new InsertionHeuristic()
            {
                MaxBreakExtensionUnits = 40,
                MaxInsertedPerBreak = 5,
                MaxLoops = 5,
                TimeLimit = new TimeSpan(0, 0, 40),
            };
            BeginingsHeuristic beginingsHeuristic = new BeginingsHeuristic()
            {
                MaxBreakExtensionUnits = 70,
                MaxLoops = 3,
                TimeLimit = new TimeSpan(0, 0, 30),
            };
            EndingsHeuristic endingHeuristic = new EndingsHeuristic()
            {
                MaxBreakExtensionUnits = 100,
                ScoringFunction = new Scorer(),
                MaxLoops = 3,
                TimeLimit = new TimeSpan(0, 0, 30),
                PropagateRandomSeed = true,
                DiagnosticMessages = true,
                Seed = 10,
                Description = "insertion_starts_ends_heuristic",
            };
            endingHeuristic.InitialSolvers.Add(randomSolver);
            endingHeuristic.InitialSolvers.Add(insertionHeuristic);
            endingHeuristic.InitialSolvers.Add(beginingsHeuristic);
            return endingHeuristic;
        }

        private static ISolver LocalRandomComplex()
        {
            FastGreedyHeuristic randomSolver = new FastGreedyHeuristic()
            {
                MaxOverfillUnits = -10,
            };
            InsertionHeuristic insertionHeuristic = new InsertionHeuristic()
            {
                MaxBreakExtensionUnits = 30,
                MaxInsertedPerBreak = 5,
                MaxLoops = 6,
                TimeLimit = new TimeSpan(0, 0, 60),
            };
            BeginingsHeuristic beginingsHeuristic = new BeginingsHeuristic()
            {
                MaxBreakExtensionUnits = 50,
                MaxLoops = 4,
                TimeLimit = new TimeSpan(0, 0, 30),
            };
            EndingsHeuristic endingHeuristic = new EndingsHeuristic()
            {
                MaxBreakExtensionUnits = 70,
                MaxLoops = 4,
                TimeLimit = new TimeSpan(0, 0, 30),
            };
            LocalSearchSolver solver = new LocalSearchSolver()
            {
                Solution = randomSolver.Solution,
                Seed = 10,
                ScoringFunction = new Scorer(),
                StopWhenCompleted = true,
                PropagateRandomnessSeed = true,
                TimeLimit = new TimeSpan(0, 0, 300),
                Description = "local_random_complex1",
            };
            solver.MoveFactories = new List<IMoveFactory>
            {
                new InsertMoveFactory()
                {
                    MildlyRandomOrder = true,
                    PositionsCountLimit = 5,
                    MaxTasksChecked = 4,
                    MaxBreaksChecked = 4,
                    IgnoreBreaksWhenUnitOverfillAbove = 60,
                    IgnoreCompletedTasks = true,
                    IgnoreTasksWithCompletedViews = false,
                },
                new InsertMoveFactory()
                {
                    MildlyRandomOrder = true,
                    PositionsCountLimit = 5,
                    MaxTasksChecked = 4,
                    MaxBreaksChecked = 4,
                    IgnoreBreaksWhenUnitOverfillAbove = 60,
                    IgnoreCompletedTasks = false,
                    IgnoreTasksWithCompletedViews = false,
                },
                new DeleteMoveFactory()
                {
                    MildlyRandomOrder = true,
                    PositionsCountLimit = 4,
                    MaxBreaksChecked = 5,
                },
                new SwapMoveFactory()
                {
                    MildlyRandomOrder = true,
                    PositionsCountLimit = 5,
                    MaxTasksChecked = 5,
                    MaxBreaksChecked = 5,
                },
            };
            solver.InitialSolvers.Add(randomSolver);
            solver.InitialSolvers.Add(insertionHeuristic);
            solver.InitialSolvers.Add(beginingsHeuristic);
            solver.InitialSolvers.Add(endingHeuristic);
            return solver;
        }

        private static ISolver GenerateFastRandomGreedyConfig()
        {
            FastGreedyHeuristic randomSolver = new FastGreedyHeuristic()
            {
                MaxOverfillUnits = -10,
                ScoringFunction = new Scorer(),
                PropagateRandomSeed = true,
                Seed = 10,
                Description = "heuristic_fast_random",
            };
            return randomSolver;
        }
    }
}
