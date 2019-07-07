using InstanceGenerator;
using InstanceGenerator.DataAccess;
using InstanceGenerator.InstanceData;
using InstanceGenerator.InstanceModification;
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
    public class BulkInstanceGenerator
    {
        public string MainDirectory { get; set; } = @"C:\Users\bartl\Desktop\MDP";
        private string RealDataDirectory => Path.Combine(MainDirectory, "pre_instances");
        private string InstanceDIrectory => Path.Combine(MainDirectory, "instances");
        private string ExampleSolutionDirectory => Path.Combine(MainDirectory, "example_solutions");
        private string ExampleSolutionScoredDirectory => Path.Combine(MainDirectory, "example_solutions_scored");

        private void GenerateInstance(string pathIn, string pathOut, RealInstanceToProblemConverter converter)
        {
            Generator instanceGenerator = new Generator
            {
                SourcePath = pathIn,
                OutputFilename = pathOut,
                InstanceConverter = converter,
            };
            instanceGenerator.GenerateInstance();
        }

        private void GenerateSolution(string pathIn, string pathOut)
        {
            Generator instanceGenerator = new Generator
            {
                SourcePath = pathIn,
                OutputFilename = pathOut,
            };
            instanceGenerator.GenerateSolution(SolutionSerializationMode.Bare);
        }

        private void GradeSolution(string instancePath, string solutionPath, string pathOut)
        {
            var deserializer = new InstanceJsonSerializer
            {
                Path = instancePath,
            };
            Instance instance = deserializer.DeserializeInstance();
            deserializer.Path = solutionPath;
            deserializer.Reader = null;
            Solution solution = deserializer.DeserializeSolution(instance);
            solution.GradingFunction = new Scorer();
            solution.GradingFunction.AssesSolution(solution);
            InstanceJsonSerializer serializer = new InstanceJsonSerializer()
            {
                Path = pathOut,
            };
            serializer.SerializeSolution(solution, SolutionSerializationMode.DebugTaskData);
        }

        private void GenerateInstancesFromDirectory(DirectoryInfo directory, string difficultyLevel, RealInstanceToProblemConverter converter)
        {
            StreamReader descFile = new StreamReader(Path.Combine(directory.FullName, "desc.txt"));
            string desc = descFile.ReadToEnd();
            converter.InstanceDescription += "\n" + desc;
            foreach (var file in directory.GetFiles())
            {
                if (file.Name == "desc.txt") continue;
                string newName = Path.GetFileNameWithoutExtension(file.Name) + ".json";
                string newInstanceName = Path.Combine(InstanceDIrectory, difficultyLevel, directory.Name, newName);
                GenerateInstance(file.FullName, newInstanceName, converter);
                string newSolutionName = Path.Combine(ExampleSolutionDirectory, difficultyLevel, directory.Name, newName);
                GenerateSolution(file.FullName, newSolutionName);
                string newGradedSolutionName = Path.Combine(ExampleSolutionScoredDirectory, difficultyLevel, directory.Name, newName);
                GradeSolution(newInstanceName, newSolutionName, newGradedSolutionName);
            }
        }


        private void GenerateInstancesForDifficulty(string difficultyLevel, RealInstanceToProblemConverter converter)
        {
            DirectoryInfo initial_dir = new DirectoryInfo(RealDataDirectory);
            var directories = initial_dir.GetDirectories();
            foreach (var dir in directories)
            {
                GenerateInstancesFromDirectory(dir, difficultyLevel, converter);
            }
        }

        public void GenerateAllInstances()
        {
            Console.WriteLine("extreme difficulty start");
            RealInstanceToProblemConverter conv = new RealInstanceToProblemConverter()
            {
                InstanceDescription = @"Extreme difficulty.",
            };
            GenerateInstancesForDifficulty("extreme", conv);

            Console.WriteLine("hard difficulty start");
            conv.InstanceDescription = @"Hard difficulty.";
            conv.MinBeginingsMultiplier = 0.7;
            conv.MinEndsMultiplier = 0.7;
            conv.MinViewsMultiplier = 0.7;
            conv.MinTimesAiredMultiplier = 0.7;
            conv.MinTimesAiredOffset = -1;
            conv.MaxAdsPerBreakOffset = 1;
            conv.DefaultAdsInBetweenSame = 10;
            GenerateInstancesForDifficulty("hard", conv);

            Console.WriteLine("medium difficulty start");
            conv.InstanceDescription = @"Medium difficulty.";
            conv.MinBeginingsOffset = -2;
            conv.MinBeginingsMultiplier = 0.5;
            conv.MinEndsOffset = -2;
            conv.MinEndsMultiplier = 0.5;
            conv.MinViewsMultiplier = 0.5;
            conv.MinTimesAiredMultiplier = 0.5;
            GenerateInstancesForDifficulty("medium", conv);

            Console.WriteLine("easy difficulty start");
            conv.InstanceDescription = @"Easy difficulty.";
            conv.MinBeginingsMultiplier = 0.3;
            conv.MinEndsMultiplier = 0.3;
            conv.MinViewsMultiplier = 0.3;
            conv.MinTimesAiredMultiplier = 0.3;
            GenerateInstancesForDifficulty("easy", conv);

            Console.WriteLine("trivial difficulty start");
            conv.InstanceDescription = @"Trivial difficulty.";
            conv.MakeEverythingCompatible = true;
            GenerateInstancesForDifficulty("trivial", conv);
        }
    }
}
