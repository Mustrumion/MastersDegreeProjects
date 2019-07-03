﻿using InstanceGenerator;
using InstanceGenerator.DataAccess;
using InstanceGenerator.InstanceData;
using InstanceGenerator.InstanceModification;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using InstanceSolvers;
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
        private static string MAIN_DIRECTORY = @"C:\MDP";
        private static string PRE_INSTANCE_DIRECTORY => Path.Combine(MAIN_DIRECTORY, "pre_instances");
        private static string INSTANCE_DIRECTORY => Path.Combine(MAIN_DIRECTORY, "instances");
        private static string EXAMPLE_SOLUTION_DIRECTORY => Path.Combine(MAIN_DIRECTORY, "example_solutions");
        private static string EXAMPLE_SOLUTION_SCORED_DIRECTORY => Path.Combine(MAIN_DIRECTORY, "example_solutions_scored");




        static void Main(string[] args)
        {
            //BulkInstanceGenerator bulkInstanceGenerator = new BulkInstanceGenerator()
            //{
            //    MainDirectory = MAIN_DIRECTORY,
            //};
            //bulkInstanceGenerator.GenerateAllInstances();

            SolveEverything("local_fast_heur", GenerateLocalSearchSolver);
        }

        private static ISolver GenerateLocalSearchSolver()
        {
            FastGreedyHeuristic randomSolver = new FastGreedyHeuristic()
            {
                MaxOverfillUnits = 60,
            };
            LocalSearchSolver solver = new LocalSearchSolver()
            {
                InitialSolver = randomSolver,
                Solution = randomSolver.Solution,
                Seed = 10,
                ScoringFunction = new Scorer(),
                StopWhenCompleted = true,
                MaxTime = new TimeSpan(0, 0, 300),
            };
            return solver;
        }

        private static void SolveEverything(string solverName, Func<ISolver> solverMaker)
        {
            DirectoryInfo initial_dir = new DirectoryInfo(INSTANCE_DIRECTORY);
            var directories = initial_dir.GetDirectories();
            Parallel.ForEach(directories, dir =>
            {
                SolveParentDirectory(dir, Path.Combine(solverName, dir.Name), solverMaker);
            });
        }

        private static void SolveParentDirectory(DirectoryInfo directory, string solverDir, Func<ISolver> solverMaker)
        {
            Parallel.ForEach(directory.GetDirectories(), childDir =>
            {
                SolveFromDirectory(childDir, Path.Combine(solverDir, childDir.Name), solverMaker);
            });
        }

        private static void SolveFromDirectory(DirectoryInfo directory, string solverDir, Func<ISolver> solverMaker)
        {
            Parallel.ForEach(directory.GetFiles(), file =>
            {
                string solutionName = Path.Combine(MAIN_DIRECTORY, solverDir, file.Name);
                Solve(file.FullName, solutionName, solverMaker());
            });
        }

        private static void Solve(string pathIn, string pathOut, ISolver solver)
        {
            var reader = new InstanceJsonSerializer
            {
                Path = pathIn,
            };
            Instance instance = reader.DeserializeInstance();
            solver.Instance = instance;
            solver.Solve();
            InstanceJsonSerializer serializer = new InstanceJsonSerializer()
            {
                Path = pathOut,
            };
            serializer.SerializeSolution(solver.Solution, SolutionSerializationMode.DebugTaskData);
            Console.WriteLine($"Solution {pathOut} was generated, completion {solver.Solution.CompletionScore}, loss {solver.Solution.WeightedLoss}, time {solver.Solution.TimeElapsed}.");
        }
    }
}
