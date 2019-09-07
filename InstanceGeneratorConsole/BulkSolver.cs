using InstanceGenerator;
using InstanceGenerator.DataAccess;
using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using InstanceSolvers;
using InstanceSolvers.Solvers.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGeneratorConsole
{
    public class BulkSolver
    {
        public volatile BulkSolverStats _stats;
        private volatile Dictionary<string, BulkSolverStats> _categorizedStats = new Dictionary<string, BulkSolverStats>();

        public string MainDirectory { get; set; }
        public string SavedSubpath { get; set; }
        public string StartingSolutionsDirectory { get; set; }
        private string InstanceDirectory => Path.Combine(MainDirectory, "instances");

        public bool ParallelExecution { get; set; } = false;
        public bool ReportProgrssToFile { get; set; } = false;
        public int MaxThreads { get; set; } = 15;
        public string[] DifficultyFilter { get; set; }
        public string[] KindFilter { get; set; }
        public string[] LengthFilter { get; set; }
        public string[] TotalStatsCategories { get; set; } = new string[] { };
        public int Times { get; set; } = 1;
        public int FirstNumber { get; set; } = 0;
        public Random Random { get; set; } = new Random();


        private string SolutionsDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(SavedSubpath))
                {
                    return Path.Combine(MainDirectory, "solutions");
                }
                return Path.Combine(MainDirectory, SavedSubpath, "solutions");
            }
        }


        private BulkSolverStats InitializeStats()
        {
            BulkSolverStats stats = new BulkSolverStats();
            stats.RepositoryVersionHash = ShellExec("git describe --always --dirty");
            return stats;
        }


        public void SolveEverything(Func<ISolver> solverMaker)
        {
            _stats = InitializeStats();
            _categorizedStats = new Dictionary<string, BulkSolverStats>();
            var solveTasks = GenerateAllTasks(solverMaker);
            solveTasks.Shuffle(Random);
            if (ParallelExecution)
            {
                Parallel.ForEach(
                    solveTasks,
                    new ParallelOptions { MaxDegreeOfParallelism = MaxThreads, }, 
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
            return directory.GetFiles().Where(d => LengthFilter == null || LengthFilter.Contains(d.Name)).SelectMany(file =>
            {
                List<Action> tasks = new List<Action>();
                for (int i = FirstNumber; i < FirstNumber + Times; i++)
                {
                    var solver = solverMaker();
                    int seed = (int)(((long)solver.Seed + Random.Next()) % int.MaxValue);
                    string solutionName = Path.Combine(SolutionsDirectory, solver.Description, solverDir, $"{i}{file.Name}");
                    tasks.Add(GenerateSolveTask(file.FullName, solutionName, solverMaker, seed));
                }
                return tasks;
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


        private List<Solution> LoadStartingSolutions(string solutionsDirectory, string currentInstance, Instance instance, int amount, ISolver solver)
        {
            string instancePath = currentInstance.Replace(InstanceDirectory + Path.DirectorySeparatorChar, "");
            string nameToSearch = Path.GetFileNameWithoutExtension(instancePath);
            string relativePathToSearch = instancePath.Replace(Path.GetFileName(instancePath), "");
            string dirToSearch = Path.Combine(StartingSolutionsDirectory, relativePathToSearch);
            string[] filePaths = Directory.GetFiles(dirToSearch, $"*{nameToSearch}*.json");

            var solutionList = new List<Solution>();
            solutionList.Shuffle(Random);

            for (int i = 0; i < Math.Min(amount, filePaths.Count()); ++i)
            {
                var deserializer = new InstanceJsonSerializer
                {
                    Reader = new StreamReader(filePaths[i]),
                };
                Solution solution = deserializer.DeserializeSolution(instance);
                solution.Description = Path.GetFileNameWithoutExtension(filePaths[i]);
                solution.GradingFunction = solver.ScoringFunction.GetAnotherOne();
                solution.GradingFunction.AssesSolution(solution);
                solutionList.Add(solution);
            }
            return solutionList;
        }
        

        private Action GenerateSolveTask(string pathIn, string pathOut, Func<ISolver> solverMaker, int seed)
        {
            return () =>
            {
                try
                {
                    string category = TotalStatsCategories.FirstOrDefault(c => pathOut.Contains(c));
                    BulkSolverStats categoryStats = null;
                    if (!string.IsNullOrEmpty(category))
                    {
                        lock (_categorizedStats)
                        {
                            if (!_categorizedStats.TryGetValue(category, out categoryStats))
                            {
                                categoryStats = InitializeStats();
                                _categorizedStats[category] = categoryStats;
                            }
                        }
                    }
                    var reader = new InstanceJsonSerializer
                    {
                        Path = pathIn,
                    };
                    Instance instance = reader.DeserializeInstance();
                    var solver = solverMaker();
                    solver.Seed = seed;
                    solver.Instance = instance;
                    if (StartingSolutionsDirectory != null)
                    {
                        if(solver is InstanceSolvers.Solvers.Evolutionary evo)
                        {
                            evo.Population = LoadStartingSolutions(SolutionsDirectory, pathIn, instance, evo.PopulationCount, solver);
                            evo.PopulationCount = evo.Population.Count;
                        }
                        else
                        {
                            solver.Solution = LoadStartingSolutions(SolutionsDirectory, pathIn, instance, 1, solver)[0];
                        }
                    }
                    IReporter reporter = new NullReporter();
                    if (ReportProgrssToFile)
                    {
                        reporter = new ScoreReporter();
                    }
                    reporter.Start();
                    solver.Reporter = reporter;
                    solver.Solve();
                    InstanceJsonSerializer serializer = new InstanceJsonSerializer()
                    {
                        Path = pathOut,
                    };
                    serializer.SerializeSolution(solver.Solution, SolutionSerializationMode.DebugTaskData);
                    reporter.Save(Path.Combine(new FileInfo(pathOut).Directory.FullName, $"{Path.GetFileNameWithoutExtension(new FileInfo(pathOut).Name)}Report.csv"));
                    Console.WriteLine($"Solution {pathOut} was generated, completion {solver.Solution.CompletionScore}, loss {solver.Solution.WeightedLoss}, time {solver.Solution.TimeElapsed.ToString(@"hh\:mm\:ss")}.");
                    AddSolutionToStats(_stats, solver);
                    AddSolutionToStats(categoryStats, solver);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
                //GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
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
