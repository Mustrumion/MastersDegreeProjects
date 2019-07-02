using InstanceGenerator;
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
        private static string MAIN_DIRECTORY = @"C:\Users\bartl\Desktop\MDP";
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

            RandomFastSolver randomSolver = new RandomFastSolver()
            {
                Seed = 10,
                ScoringFunction = new Scorer(),
            };
            LocalRandomSearch solver = new LocalRandomSearch()
            {
                InitialSolver = randomSolver,
                Solution = randomSolver.Solution,
                Seed = 10,
                ScoringFunction = new Scorer(),
                MaxTime = new TimeSpan(0, 0, 30),
                StopWhenCompleted = true,
            };
            SolveEverything("local_random", solver);
        }


        private static void SolveEverything(string solverName, ISolver solver)
        {
            DirectoryInfo initial_dir = new DirectoryInfo(INSTANCE_DIRECTORY);
            var directories = initial_dir.GetDirectories();
            foreach (var dir in directories)
            {
                SolveParentDirectory(dir, Path.Combine(solverName, dir.Name), solver);
            }
        }

        private static void SolveParentDirectory(DirectoryInfo directory, string solverDir, ISolver solver)
        {
            foreach (var childDir in directory.GetDirectories())
            {
                SolveFromDirectory(childDir, Path.Combine(solverDir, childDir.Name), solver);
            }
        }

        private static void SolveFromDirectory(DirectoryInfo directory, string solverDir, ISolver solver)
        {
            foreach (var file in directory.GetFiles())
            {
                string solutionName = Path.Combine(MAIN_DIRECTORY, solverDir, file.Name);
                Solve(file.Name, solutionName, solver);
            }
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
        }

    }
}
