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
        private volatile Dictionary<string, BulkSolverStats> _categorizedStats = new Dictionary<string, BulkSolverStats>();

        public string MainDirectory { get; set; } = @"C:\Users\bartl\Desktop\MDP";
        private string InstanceDirectory => Path.Combine(MainDirectory, "instances");
        private string SolutionsDirectory => Path.Combine(MainDirectory, "solutions");
        public bool ParallelExecution { get; set; } = false;
        public int MaxThreads { get; set; } = 4;
        public string[] DifficultyFilter { get; set; }
        public string[] KindFilter { get; set; }
        public string[] LengthFilter { get; set; }
        public string[] TotalStatsCategories { get; set; }


        private BulkSolverStats GenerateInitialStats()
        {
            BulkSolverStats stats = new BulkSolverStats();
            stats.RepositoryVersionHash = ShellExec("git describe --always --dirty");
            return stats;
        }


        public void SolveEverything(Func<ISolver> solverMaker)
        {
            _stats = GenerateInitialStats();
            _categorizedStats = new Dictionary<string, BulkSolverStats>();
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
            SerializeStats(_stats, statsPath);
            foreach(var categoryStats in _categorizedStats)
            {
                statsPath = Path.Combine(SolutionsDirectory, solver.Description, "CategorizedStats", $"{categoryStats.Key}.json");
                SerializeStats(categoryStats.Value, statsPath);
            }
        }

        private void SerializeStats(BulkSolverStats stats, string statsPath)
        {
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
                ser.Serialize(writer, stats);
                writer.Flush();
                writer.Close();
            }
        }


        private List<Action> GenerateAllTasks(Func<ISolver> solverMaker)
        {
            DirectoryInfo initial_dir = new DirectoryInfo(InstanceDirectory);
            var directories = initial_dir.GetDirectories();
            return directories.Where(d => DifficultyFilter == null || DifficultyFilter.Contains(d.Name)).SelectMany(dir =>
            {
                return GenerateTasksParentDirectory(dir, dir.Name, solverMaker);
            }).ToList();
        }

        private List<Action> GenerateTasksParentDirectory(DirectoryInfo directory, string solverDir, Func<ISolver> solverMaker)
        {
            return directory.GetDirectories().Where(d => KindFilter == null || KindFilter.Contains(d.Name)).SelectMany(childDir =>
            {
                return GenerateTasksFromDirectory(childDir, Path.Combine(solverDir, childDir.Name), solverMaker);
            }).ToList();
        }

        private List<Action> GenerateTasksFromDirectory(DirectoryInfo directory, string solverDir, Func<ISolver> solverMaker)
        {
            return directory.GetFiles().Where(d => LengthFilter == null || LengthFilter.Contains(d.Name)).Select(file =>
            {
                var solver = solverMaker();
                string solutionName = Path.Combine(SolutionsDirectory, solver.Description, solverDir, file.Name);
                return GenerateSolveTask(file.FullName, solutionName, solver);
            }).ToList();
        }
        
        private void AddSolutionToStats(BulkSolverStats stats, ISolver solver)
        {
            if (stats == null) return;
            lock (stats)
            {
                stats.TotalTime += solver.Solution.TimeElapsed;
                stats.NumberOfExamples += 1;
                stats.NumberOfAcceptableSolutions += solver.Solution.CompletionScore >= 1 ? 1 : 0;
                stats.TasksStats.AddTasksStats(solver.Solution.TotalStats);
            }
        }
        
        private Action GenerateSolveTask(string pathIn, string pathOut, ISolver solver)
        {
            return () =>
            {
                string category = TotalStatsCategories.FirstOrDefault(c => pathOut.Contains(c));
                BulkSolverStats categoryStats = null;
                if (!string.IsNullOrEmpty(category))
                {
                    lock (_categorizedStats)
                    {
                        if (!_categorizedStats.TryGetValue(category, out categoryStats))
                        {
                            categoryStats = GenerateInitialStats();
                            _categorizedStats[category] = categoryStats;
                        }
                    }
                }
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
                AddSolutionToStats(_stats, solver);
                AddSolutionToStats(categoryStats, solver);
            };
        }
    

        private string ShellExec(string command, string workingDir = null)
        {
            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo.FileName = "cmd.exe";
            pProcess.StartInfo.Arguments = $"/C \"{command}\"";
            if (!string.IsNullOrWhiteSpace(workingDir))
            {
                pProcess.StartInfo.WorkingDirectory = workingDir;
            }
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.Start();
            string strOutput = pProcess.StandardOutput.ReadToEnd();
            pProcess.WaitForExit();
            return strOutput;
        }
    }
}
