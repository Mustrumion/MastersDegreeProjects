using InstanceGenerator.DataAccess;
using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using Newtonsoft.Json;
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
        public volatile BulkSolverStats _stats;

        public string MainDirectory { get; set; } = @"C:\Users\bartl\Desktop\MDP";
        private string InstanceDirectory => Path.Combine(MainDirectory, "instances");
        private string SolutionsDirectory => Path.Combine(MainDirectory, "solutions");
        public bool ParallelExecution { get; set; } = false;
        public int MaxThreads { get; set; } = 4;

        public void SolveEverything(Func<ISolver> solverMaker)
        {
            _stats = new BulkSolverStats();
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

            ISolver solver = solverMaker();
            _stats.Solver = solver;
            string statsPath = Path.Combine(SolutionsDirectory, solver.Description, "TotalStats.json");
            FileInfo file = new FileInfo(statsPath);
            if (!file.Directory.Exists)
            {
                file.Directory.Create();
            }
            using (var writer = new StreamWriter(statsPath))
            {
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.None,
                    Formatting = Formatting.Indented,
                    TypeNameHandling = TypeNameHandling.Objects,
                };
                JsonSerializer ser = JsonSerializer.Create(settings);
                ser.Serialize(writer, _stats);
                writer.Flush();
                writer.Close();
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
                _stats.TotalTime += solver.Solution.TimeElapsed;
                _stats.NumberOfExamples += 1;
                _stats.TasksStats.AddTasksStats(solver.Solution.TotalStats);
            };
        }
    
    }
}
