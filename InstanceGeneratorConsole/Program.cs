using InstanceGenerator;
using InstanceGenerator.DataAccess;
using InstanceGenerator.InstanceData;
using InstanceGenerator.InstanceModification;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using InstanceSolvers;
using InstanceSolvers.TransformationFactories;
using InstanceSolvers.Solvers;
using InstanceSolvers.Solvers.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGeneratorConsole
{
    class Program
    {
        const uint ENABLE_QUICK_EDIT = 0x0040;

        // STD_INPUT_HANDLE (DWORD): -10 is the standard input device.
        const int STD_INPUT_HANDLE = -10;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);


        private static string MAIN_DIRECTORY = @"C:\Users\mustrum\dropbox\MDP";


        static void Main(string[] args)
        {
            IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);
            uint consoleMode;
            if (!GetConsoleMode(consoleHandle, out consoleMode))
            {
                Console.WriteLine("Can't get console handle. Weird.");
                return;
            }
            consoleMode &= ~ENABLE_QUICK_EDIT;
            if (!SetConsoleMode(consoleHandle, consoleMode))
            {
                Console.WriteLine("Can't set console mode. Weird.");
            }
            //BulkInstanceGenerator bulkInstanceGenerator = new BulkInstanceGenerator()
            //{
            //    MainDirectory = MAIN_DIRECTORY,
            //};
            //bulkInstanceGenerator.GenerateAllInstances();

            //BulkSolver bulkSolver = new BulkSolver()
            //{
            //    MainDirectory = MAIN_DIRECTORY,
            //    ParallelExecution = false,
            //    Times = 2,
            //    MaxThreads = 4,
            //    TotalStatsCategories = new[] { "trivial", "very_easy", "easy", "medium", "hard", "extreme" },
            //    DifficultyFilter = new[] { "medium" },
            //    KindFilter = new[] { "4ch3n1a1" },
            //    LengthFilter = new[] { "week.json" },
            //};

            //bulkSolver.SolveEverything(LocalSearchNewStopConditions);
            //bulkSolver.SolveEverything(SimulatedAnnealingGenerator);

            //DifficultyChoiceExperiment experiment = new DifficultyChoiceExperiment();
            //experiment.Perform();

            SimulatedAnnealingTuning experiment = new SimulatedAnnealingTuning();
            experiment.Perform();

            Console.WriteLine("Press any key.");
            Console.ReadKey();
        }

        private static ISolver InsertionStartEndingDeleteConfiguration()
        {
            GreedyFastHeuristic randomSolver = new GreedyFastHeuristic();
            ViewsHeuristic insertionHeuristic = new ViewsHeuristic()
            {
                MaxBreakExtensionUnits = 40,
                MaxLoops = 6,
                MaxInsertedPerBreak = 5,
            };
            BeginingsHeuristic beginingsHeuristic = new BeginingsHeuristic()
            {
                MaxLoops = 6,
                MaxBreakExtensionUnits = 999,
            };
            EndingsHeuristic endingHeuristic = new EndingsHeuristic()
            {
                MaxLoops = 6,
                MaxBreakExtensionUnits = 999,
            };
            FreeSpaceHeuristic freeSpaceHeuristic = new FreeSpaceHeuristic()
            {
                ScoringFunction = new Scorer(),
                MaxLoops = 6,
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


        private static ISolver EvolutionarySolver()
        {
            Evolutionary solver = new Evolutionary()
            {
                Description = "evolutionary2",
                ScoringFunction = new Scorer(),
                DiagnosticMessages = true,
                PropagateRandomSeed = true,
                ParallelAllowed = true,
                Seed = 10,
                GenerationImproverGenerator = LocalSearchForEvolutionaryImprovement,
                GenerationCreatorGenerator = LocalSearchEvolutionaryInitial,
            };
            return solver;
        }



        private static ISolver LocalSearchNewStopConditions2()
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
                Description = "local_search_new_stop_condition_15rs4",
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


        private static ISolver SimulatedAnnealingGenerator()
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
            SimulatedAnnealing simulatedAnnealing = new SimulatedAnnealing()
            {
                DiagnosticMessages = true,
                ScoringFunction = new Scorer(),
                StepsAnalyzedWithoutImprovementToStop = 200,
                NumberOfLoopsWithoutActionsToStop = 1,
                PropagateRandomSeed = true,
                Seed = 100,
                Description = "SimulatedAnnealing",
                IntegrityLossMultiplier = 1000.0,
            };
            simulatedAnnealing.InitialSolvers.Add(randomSolver);
            simulatedAnnealing.InitialSolvers.Add(compundSolver);
            return simulatedAnnealing;
        }



        private static ISolver LocalSearchEvolutionaryInitial()
        {
            GreedyFastHeuristic randomSolver = new GreedyFastHeuristic()
            {
                MaxOverfillUnits = 0,
            };
            CompoundSolver compundSolver = new CompoundSolver()
            {
                MaxLoops = 3,
            };
            LocalSearch solver = new LocalSearch()
            {
                PropagateRandomSeed = true,
                NumberOfNoGoodActionsToStop = 7,
                BestFactoryAdjustmentParam = 0.1,
                NeighberhoodAdjustmentParam = 0.1,
                ImprovementOverNarrowNeighb = 1.5,
                TimeLimit = new TimeSpan(0, 5, 0),
                DiagnosticMessages = true,
                ReportTimeouts = true,
            };
            solver.MoveFactories = new List<ITransformationFactory>
            {
                new InsertFactory()
                {
                    MildlyRandomOrder = true,
                    PositionsCountLimit = 3,
                    MaxTasksChecked = 1,
                    MaxBreaksChecked = 1,
                    IgnoreBreaksWhenUnitOverfillAbove = 60,
                    IgnoreCompletedTasks = true,
                    IgnoreTasksWithCompletedViews = false,
                    AlwaysReturnStartsAndEnds = true,
                },
                new RandomDeleteFactory()
                {
                    MovesReturned = 20,
                    RampUpSpeed = 3.0,
                },
                new RandomInsertFactory()
                {
                    MovesReturned = 20,
                    RampUpSpeed = 3.0,
                },
                new RandomSwapFactory()
                {
                    MovesReturned = 40,
                    RampUpSpeed = 4.0,
                },
            };
            solver.InitialSolvers.Add(randomSolver);
            solver.InitialSolvers.Add(compundSolver);
            return solver;
        }

        private static ISolver LocalSearchForEvolutionaryImprovement()
        {
            CompoundSolver compundSolver = new CompoundSolver()
            {
                MaxLoops = 1,
            };
            LocalSearch solver = new LocalSearch()
            {
                NumberOfNoGoodActionsToStop = 15,
                BestFactoryAdjustmentParam = 0.1,
                NeighberhoodAdjustmentParam = 0.1,
                ImprovementOverNarrowNeighb = 1.5,
                TimeLimit = new TimeSpan(0, 2, 0),
                ReportTimeouts = true,
            };
            solver.MoveFactories = new List<ITransformationFactory>
            {
                new InsertFactory()
                {
                    MildlyRandomOrder = true,
                    PositionsCountLimit = 3,
                    MaxTasksChecked = 1,
                    MaxBreaksChecked = 1,
                    IgnoreBreaksWhenUnitOverfillAbove = 60,
                    IgnoreCompletedTasks = true,
                    IgnoreTasksWithCompletedViews = false,
                    AlwaysReturnStartsAndEnds = true,
                },
                new RandomDeleteFactory()
                {
                    MovesReturned = 20,
                    RampUpSpeed = 3.0,
                },
                new RandomInsertFactory()
                {
                    MovesReturned = 20,
                    RampUpSpeed = 3.0,
                },
                new RandomSwapFactory()
                {
                    MovesReturned = 50,
                    RampUpSpeed = 4.0,
                },
            };
            solver.InitialSolvers.Add(compundSolver);
            return solver;
        }


        private static ISolver LocalSearchNewStopConditions()
        {
            GreedyFastHeuristic randomSolver = new GreedyFastHeuristic()
            {
                MaxOverfillUnits = 0,
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
                NumberOfNoGoodActionsToStop = 10,
                TimeLimit = new TimeSpan(0, 1, 0),
                BestFactoryAdjustmentParam = 0.2,
                NeighberhoodAdjustmentParam = 0.2,
                ImprovementOverNarrowNeighb = 2,
                Description = "local_search_new_stop_condition_1",
            };
            solver.InitialSolvers.Add(randomSolver);
            solver.InitialSolvers.Add(compundSolver);
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

        private static ISolver OldRandom()
        {
            RandomInsertsSolver randomSolver = new RandomInsertsSolver()
            {
                ScoringFunction = new Scorer(),
                TimeLimit = new TimeSpan(0, 0, 20),
                PropagateRandomSeed = true,
                Seed = 10,
                Description = "old_random",
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


        private static ISolver OldGreedy()
        {
            ImprovingInsertsHeuristic randomSolver = new ImprovingInsertsHeuristic()
            {
                ScoringFunction = new Scorer(),
                TimeLimit = new TimeSpan(0, 0, 200),
                PropagateRandomSeed = true,
                MaxBreakExtensionUnits = 1,
                PositionsPerBreakTakenIntoConsideration = 3,
                Seed = 10,
                Description = "old_greedy",
            };
            return randomSolver;
        }
    }
}
