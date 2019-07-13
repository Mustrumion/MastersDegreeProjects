using InstanceGenerator;
using InstanceGenerator.DataAccess;
using InstanceGenerator.InstanceData;
using InstanceGenerator.InstanceModification;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using InstanceSolvers;
using InstanceSolvers.MoveFactories;
using InstanceSolvers.Solvers;
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
        private static string MAIN_DIRECTORY = @"C:\Users\Mustrum\dropbox\MDP";


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
                MaxThreads = 15,
                TotalStatsCategories = new[] { "trivial", "very_easy", "easy", "medium", "hard", "extreme" },
                //    DifficultyFilter = new[] { "extreme" },
                //    KindFilter = new[] { "3edu2" },
                //    LengthFilter = new[] { "month.json" },
            };

            bulkSolver.SolveEverything(FastRandomConfig);
            bulkSolver.SolveEverything(InsertionStartEndingDeleteConfiguration);
            bulkSolver.SolveEverything(LocalSearchBasedInCompundConfiguration);
            bulkSolver.SolveEverything(LocalSearchConfiguration2);
            bulkSolver.SolveEverything(LocalSearchConfiguration3);
            bulkSolver.SolveEverything(InsertionConfiguration);
            bulkSolver.SolveEverything(StartInsertionConfiguration);
            bulkSolver.SolveEverything(InsertionStartEndingConfiguration);
            bulkSolver.SolveEverything(CompundConfiguration);
            bulkSolver.SolveEverything(LocalRandomComplex);
            bulkSolver.SolveEverything(FastRandomGreedyConfig);
            bulkSolver.SolveEverything(SlowRandomConfig);

            Console.WriteLine("Press any key.");
            Console.ReadKey();
        }



        private static ISolver LocalSearchConfiguration2()
        {
            GreedyFastHeuristic randomSolver = new GreedyFastHeuristic()
            {
                MaxOverfillUnits = 30,
            };
            ViewsHeuristic insertionHeuristic = new ViewsHeuristic()
            {
                MaxBreakExtensionUnits = 30,
                MaxInsertedPerBreak = 3,
            };
            LocalSearch solver = new LocalSearch()
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

        private static ISolver LocalSearchConfiguration3()
        {
            GreedyFastHeuristic randomSolver = new GreedyFastHeuristic()
            {
                MaxOverfillUnits = -10,
            };
            ViewsHeuristic insertionHeuristic = new ViewsHeuristic()
            {
                MaxBreakExtensionUnits = 30,
                MaxInsertedPerBreak = 3,
                TimeLimit = new TimeSpan(0, 0, 30),
            };
            LocalSearch solver = new LocalSearch()
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

        private static ISolver InsertionConfiguration()
        {
            GreedyFastHeuristic randomSolver = new GreedyFastHeuristic()
            {
                MaxOverfillUnits = -10,
            };
            ViewsHeuristic insertionHeuristic = new ViewsHeuristic()
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

        private static ISolver StartInsertionConfiguration()
        {
            GreedyFastHeuristic randomSolver = new GreedyFastHeuristic()
            {
                MaxOverfillUnits = -10,
            };
            ViewsHeuristic insertionHeuristic = new ViewsHeuristic()
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

        private static ISolver InsertionStartEndingConfiguration()
        {
            GreedyFastHeuristic randomSolver = new GreedyFastHeuristic()
            {
                MaxOverfillUnits = -10,
            };
            ViewsHeuristic insertionHeuristic = new ViewsHeuristic()
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

        private static ISolver InsertionStartEndingDeleteConfiguration()
        {
            GreedyFastHeuristic randomSolver = new GreedyFastHeuristic()
            {
                MaxOverfillUnits = -10,
            };
            ViewsHeuristic insertionHeuristic = new ViewsHeuristic()
            {
                MaxBreakExtensionUnits = 40,
                MaxInsertedPerBreak = 5,
                TimeLimit = new TimeSpan(0, 0, 50),
            };
            BeginingsHeuristic beginingsHeuristic = new BeginingsHeuristic()
            {
                MaxBreakExtensionUnits = 70,
                TimeLimit = new TimeSpan(0, 0, 50),
            };
            EndingsHeuristic endingHeuristic = new EndingsHeuristic()
            {
                MaxBreakExtensionUnits = 100,
                TimeLimit = new TimeSpan(0, 0, 50),
            };
            FreeSpaceHeuristic freeSpaceHeuristic = new FreeSpaceHeuristic()
            {
                ScoringFunction = new Scorer(),
                TimeLimit = new TimeSpan(0, 0, 50),
                PropagateRandomSeed = true,
                Seed = 10,
                DiagnosticMessages = true,
                Description = "views_starts_ends_trim_heuristic",
            };
            freeSpaceHeuristic.InitialSolvers.Add(randomSolver);
            freeSpaceHeuristic.InitialSolvers.Add(insertionHeuristic);
            freeSpaceHeuristic.InitialSolvers.Add(beginingsHeuristic);
            freeSpaceHeuristic.InitialSolvers.Add(endingHeuristic);
            return freeSpaceHeuristic;
        }


        private static ISolver CompundConfiguration()
        {
            GreedyFastHeuristic randomSolver = new GreedyFastHeuristic()
            {
                MaxOverfillUnits = -10,
            };
            CompoundSolver compundSolver = new CompoundSolver()
            {
                TimeLimit = new TimeSpan(0, 0, 80),
                Description = "compund_heuristic_random_70s",
                PropagateRandomSeed = true,
                Seed = 10,
                DiagnosticMessages = true,
                MaxLoops = 10,
                ScoringFunction = new Scorer(),
            };
            compundSolver.InitialSolvers.Add(randomSolver);
            return compundSolver;
        }

        private static ISolver LocalSearchBasedInCompundConfiguration()
        {
            GreedyFastHeuristic randomSolver = new GreedyFastHeuristic()
            {
                MaxOverfillUnits = -10,
            };
            CompoundSolver compundSolver = new CompoundSolver()
            {
                TimeLimit = new TimeSpan(0, 0, 150),
                Seed = 15,
                MaxLoops = 10,
                RandomOrder = true,
            };
            LocalSearch solver = new LocalSearch()
            {
                Solution = randomSolver.Solution,
                Seed = 10,
                ScoringFunction = new Scorer(),
                StopWhenCompleted = true,
                PropagateRandomnessSeed = true,
                TimeLimit = new TimeSpan(0, 0, 300),
                Description = "local_random_compound",
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
            solver.InitialSolvers.Add(compundSolver);
            return solver;
        }

        private static ISolver LocalRandomComplex()
        {
            GreedyFastHeuristic randomSolver = new GreedyFastHeuristic()
            {
                MaxOverfillUnits = -10,
            };
            ViewsHeuristic insertionHeuristic = new ViewsHeuristic()
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
            LocalSearch solver = new LocalSearch()
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

        private static ISolver FastRandomGreedyConfig()
        {
            GreedyFastHeuristic randomSolver = new GreedyFastHeuristic()
            {
                MaxOverfillUnits = -10,
                ScoringFunction = new Scorer(),
                PropagateRandomSeed = true,
                Seed = 10,
                Description = "heuristic_fast_random",
            };
            return randomSolver;
        }

        private static ISolver SlowRandomConfig()
        {
            RandomSolver randomSolver = new RandomSolver()
            {
                ScoringFunction = new Scorer(),
                PropagateRandomSeed = true,
                Seed = 10,
                Description = "pure_random_slow",
            };
            return randomSolver;
        }

        private static ISolver FastRandomConfig()
        {
            RandomFastSolver randomSolver = new RandomFastSolver()
            {
                ScoringFunction = new Scorer(),
                PropagateRandomSeed = true,
                Seed = 10,
                Description = "pure_random_fast",
            };
            return randomSolver;
        }
    }
}
