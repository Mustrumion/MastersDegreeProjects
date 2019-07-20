﻿using InstanceGenerator;
using InstanceGenerator.DataAccess;
using InstanceGenerator.InstanceData;
using InstanceGenerator.InstanceModification;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using InstanceSolvers;
using InstanceSolvers.MoveFactories;
using InstanceSolvers.Solvers;
using InstanceSolvers.Solvers.Base;
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
        private static string MAIN_DIRECTORY = @"C:\Users\bartl\dropbox\MDP";


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
                ParallelExecution = false,
                MaxThreads = 16,
                TotalStatsCategories = new[] { "trivial", "very_easy", "easy", "medium", "hard", "extreme" },
                //DifficultyFilter = new[] { "very_easy", "easy", "medium" },
                //    KindFilter = new[] { "3edu2" },
                //    LengthFilter = new[] { "month.json" },
            };

            bulkSolver.SolveEverything(LocalSearchNewStopConditions);

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

        

        private static ISolver LocalSearchNewStopConditions()
        {
            GreedyFastHeuristic randomSolver = new GreedyFastHeuristic()
            {
                MaxOverfillUnits = 0,
            };
            CompoundSolver compundSolver = new CompoundSolver()
            {
                MaxLoops = 6,
            };
            LocalSearch solver = new LocalSearch()
            {
                ScoringFunction = new Scorer(),
                DiagnosticMessages = true,
                PropagateRandomSeed = true,
                NumberOfNoGoodActionsToStop = 10,
                BestFactoryAdjustmentParam = 0.5,
                NeighberhoodAdjustmentParam = 0.5,
                Description = "local_search_new_stop_condition_10",
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
            RandomSolver randomSolver = new RandomSolver()
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
            GreedyHeuristic randomSolver = new GreedyHeuristic()
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
