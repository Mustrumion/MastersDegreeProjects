using InstanceGenerator.DataAccess;
using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGeneratorConsole
{
    public class BulkSolver
    {
        public string MainDirectory { get; set; } = @"C:\Users\bartl\Desktop\MDP";
        private string InstanceDirectory => Path.Combine(MainDirectory, "instances");
        private string SolutionsDirectory => Path.Combine(MainDirectory, "solutions");
        public bool ParallelExecution { get; set; } = false;
        public int MaxThreads { get; set; } = 4;

        public void SolveEverything(Func<ISolver> solverMaker)
        {
            var solveTasks = GenerateAllTasks(solverMaker);
            if (ParallelExecution)
            {
                Parallel.ForEach(
                    solveTasks,
                    new ParallelOptions { MaxDegreeOfParallelism = MaxThreads }, 
                    solveTask => solveTask());
            }
            else
            {
                foreach(var task in solveTasks)
                {
                    task();
                }
            }
        }


        private List<Action> GenerateAllTasks(Func<ISolver> solverMaker)
        {
            DirectoryInfo initial_dir = new DirectoryInfo(InstanceDirectory);
            var directories = initial_dir.GetDirectories();
            return directories.SelectMany(dir =>
            {
                return GenerateTasksParentDirectory(dir, dir.Name, solverMaker);
            }).ToList();
        }

        private List<Action> GenerateTasksParentDirectory(DirectoryInfo directory, string solverDir, Func<ISolver> solverMaker)
        {
            return directory.GetDirectories().SelectMany(childDir =>
            {
                return GenerateTasksFromDirectory(childDir, Path.Combine(solverDir, childDir.Name), solverMaker);
            }).ToList();
        }

        private List<Action> GenerateTasksFromDirectory(DirectoryInfo directory, string solverDir, Func<ISolver> solverMaker)
        {
            return directory.GetFiles().Select(file =>
            {
                var solver = solverMaker();
                string solutionName = Path.Combine(SolutionsDirectory, solver.Description, solverDir, file.Name);
                return GenerateSolveTask(file.FullName, solutionName, solver);
            }).ToList();
        }

        private Action GenerateSolveTask(string pathIn, string pathOut, ISolver solver)
        {
            return () =>
            {
                //Console.WriteLine($"Started {pathOut}");
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
                Console.WriteLine($"Solution {pathOut} was generated, completion {solver.Solution.CompletionScore}, loss {solver.Solution.WeightedLoss}, time {solver.Solution.TimeElapsed.ToString(@"hh\:mm\:ss")}.");
            };
        }
    
    }
}
