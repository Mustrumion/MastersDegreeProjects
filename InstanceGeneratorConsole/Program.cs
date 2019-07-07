﻿using InstanceGenerator;
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
        private static string MAIN_DIRECTORY = @"C:\Users\bartl\Dropbox\MDP";


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
            };
            bulkSolver.SolveEverything(GenerateInsertionSolverConfiguration);

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
                MaxBreakExtensionUnits = 30,
                MaxInsertedPerBreak = 5,
                ScoringFunction = new Scorer(),
                TimeLimit = new TimeSpan(0, 0, 60),
                PropagateRandomSeed = true,
                Seed = 10,
                Description = "insertion_heuristic2",
            };
            insertionHeuristic.InitialSolvers.Add(randomSolver);
            return insertionHeuristic;
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
