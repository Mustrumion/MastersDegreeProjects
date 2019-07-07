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
        public void SolveEverything(Func<ISolver> solverMaker)
        {
            DirectoryInfo initial_dir = new DirectoryInfo(InstanceDirectory);
            var directories = initial_dir.GetDirectories();
            Parallel.ForEach(directories, dir =>
            {
                SolveParentDirectory(dir, dir.Name, solverMaker);
            });
        }

        private  void SolveParentDirectory(DirectoryInfo directory, string solverDir, Func<ISolver> solverMaker)
        {
            Parallel.ForEach(directory.GetDirectories(), childDir =>
            {
                SolveFromDirectory(childDir, Path.Combine(solverDir, childDir.Name), solverMaker);
            });
        }

        private void SolveFromDirectory(DirectoryInfo directory, string solverDir, Func<ISolver> solverMaker)
        {
            Parallel.ForEach(directory.GetFiles(), file =>
            {
                var solver = solverMaker();
                string solutionName = Path.Combine(MainDirectory, solver.Description, solverDir, file.Name);
                Solve(file.FullName, solutionName, solver);
            });
        }

        private void Solve(string pathIn, string pathOut, ISolver solver)
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
        }
    
    }
}
