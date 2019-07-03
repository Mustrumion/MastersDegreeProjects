using InstanceGenerator;
using InstanceGenerator.DataAccess;
using InstanceGenerator.InstanceData;
using InstanceGenerator.InstanceModification;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using InstanceSolvers;
using InstanceSolvers.MoveFactories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SystemTestsProject
{
    [TestClass]
    public class SystemTests
    {
        [TestMethod]
        public void ReaderReadsWeekly3ChannelFileTest()
        {
            var file = Properties.Resources.week_DS_D_DH;
            using (var reader = new StringReader(file))
            {
                RealInstanceDataLoader loader = new RealInstanceDataLoader
                {
                    Reader = reader
                };
                Instance instance = loader.LoadInstanceFile();
                Assert.IsNotNull(instance);
            }
        }

        [TestMethod]
        public void DeserializeWeek3ChannelTest()
        {
            var file = Properties.Resources.week_DS_D_DH_inst;
            var reader = new InstanceJsonSerializer
            {
                Reader = new StreamReader(new MemoryStream(file), Encoding.UTF8)
            };
            Instance instance = reader.DeserializeInstance();
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void RandomSolverSolveWeek3ChannelInstance()
        {
            var file = Properties.Resources.week_DS_D_DH_inst;
            var reader = new InstanceJsonSerializer
            {
                Reader = new StreamReader(new MemoryStream(file), Encoding.UTF8)
            };
            Instance instance = reader.DeserializeInstance();
            RandomSolver solver = new RandomSolver()
            {
                Instance = instance,
                Seed = 10,
                ScoringFunction = new Scorer(),
            };
            solver.Solve();
            InstanceJsonSerializer serializer = new InstanceJsonSerializer()
            {
                Path = @"results\week_DS_D_DH_sol_random.json"
            };
            serializer.SerializeSolution(solver.Solution, SolutionSerializationMode.DebugTaskData);
        }

        [TestMethod]
        public void RandomFastSolverSolveWeek3ChannelInstance()
        {
            var file = Properties.Resources.week_DS_D_DH_inst;
            var reader = new InstanceJsonSerializer
            {
                Reader = new StreamReader(new MemoryStream(file), Encoding.UTF8)
            };
            Instance instance = reader.DeserializeInstance();
            RandomFastSolver solver = new RandomFastSolver()
            {
                Instance = instance,
                Seed = 10,
                ScoringFunction = new Scorer(),
            };
            solver.Solve();
            InstanceJsonSerializer serializer = new InstanceJsonSerializer()
            {
                Path = @"results\week_DS_D_DH_sol_randomfast.json"
            };
            serializer.SerializeSolution(solver.Solution, SolutionSerializationMode.DebugTaskData);
        }

        [TestMethod]
        public void GreedyHeuristicSolveDay3ChannelInstance()
        {
            var file = Properties.Resources.day_DS_D_DH_inst;
            var reader = new InstanceJsonSerializer
            {
                Reader = new StreamReader(new MemoryStream(file), Encoding.UTF8)
            };
            Instance instance = reader.DeserializeInstance();
            GreedyHeuristicSolver solver = new GreedyHeuristicSolver()
            {
                Instance = instance,
                Seed = 10,
                ScoringFunction = new Scorer(),
            };
            solver.Solve();
            InstanceJsonSerializer serializer = new InstanceJsonSerializer()
            {
                Path = @"results\day_DS_D_DH_sol_greedyheur.json"
            };
            serializer.SerializeSolution(solver.Solution, SolutionSerializationMode.DebugTaskData);
        }

        [TestMethod]
        public void GreedyFastHeuristicSolveDay3ChannelInstance()
        {
            var file = Properties.Resources.day_DS_D_DH_inst;
            var reader = new InstanceJsonSerializer
            {
                Reader = new StreamReader(new MemoryStream(file), Encoding.UTF8)
            };
            Instance instance = reader.DeserializeInstance();
            FastGreedyHeuristic solver = new FastGreedyHeuristic()
            {
                Instance = instance,
                Seed = 10,
                ScoringFunction = new Scorer(),
                MaxOverfillUnits = 10,
            };
            solver.Solve();
            InstanceJsonSerializer serializer = new InstanceJsonSerializer()
            {
                Path = @"results\day_DS_D_DH_sol_greedyfastheur.json"
            };
            serializer.SerializeSolution(solver.Solution, SolutionSerializationMode.DebugTaskData);
        }

        [TestMethod]
        public void LocalRandomSolveDay3ChannelInstance()
        {
            var file = Properties.Resources.day_DS_D_DH_inst;
            var reader = new InstanceJsonSerializer
            {
                Reader = new StreamReader(new MemoryStream(file), Encoding.UTF8)
            };
            Instance instance = reader.DeserializeInstance();
            RandomFastSolver randomSolver = new RandomFastSolver()
            {
                Instance = instance,
                Seed = 10,
            };
            LocalSearchSolver solver = new LocalSearchSolver()
            {
                InitialSolver = randomSolver,
                Instance = instance,
                Solution = randomSolver.Solution,
                Seed = 10,
                ScoringFunction = new Scorer(),
                StopWhenCompleted = true,
                MaxTime = new TimeSpan(0, 0, 60),
            };
            solver.Solve();
            InstanceJsonSerializer serializer = new InstanceJsonSerializer()
            {
                Path = @"results\day_DS_D_DH_sol_localrandom.json"
            };
            serializer.SerializeSolution(solver.Solution, SolutionSerializationMode.DebugTaskData);
        }


        [TestMethod]
        public void GenerateDay3ChannelsInstanceBasedOnRealData()
        {
            Generator instanceGenerator = new Generator
            {
                DataSource = new StringReader(Properties.Resources.day_DS_D_DH),
                OutputFilename = @"results\day_DS_D_DH_inst.json",
                InstanceConverter = new RealInstanceToProblemConverter()
                {
                    InstanceDescription = "3 mildly educational channels belonging to the same company."
                }
            };
            instanceGenerator.GenerateInstance();
        }


        [TestMethod]
        public void GenerateWeek3ChannelsInstanceBasedOnRealData()
        {
            Generator instanceGenerator = new Generator
            {
                DataSource = new StringReader(Properties.Resources.week_DS_D_DH),
                OutputFilename = @"results\week_DS_D_DH_inst.json",
                InstanceConverter = new RealInstanceToProblemConverter()
                {
                    InstanceDescription = "3 mildly educational channels belonging to the same company."
                }
            };
            instanceGenerator.GenerateInstance();
        }


        [TestMethod]
        public void GenerateHour3ChannelsInstanceBasedOnRealData()
        {
            Generator instanceGenerator = new Generator
            {
                DataSource = new StringReader(Properties.Resources.hour_DS_D_DH),
                OutputFilename = @"results\hour_DS_D_DH_inst.json",
                InstanceConverter = new RealInstanceToProblemConverter()
                {
                    InstanceDescription = "3 mildly educational channels belonging to the same company."
                }
            };
            instanceGenerator.GenerateInstance();
        }

        [TestMethod]
        public void GenerateWeek3ChannelSolutionBasedsOnRealData()
        {
            Generator instanceGenerator = new Generator
            {
                DataSource = new StringReader(Properties.Resources.week_DS_D_DH),
                OutputFilename = @"results\week_DS_D_DH_sol.json"
            };
            instanceGenerator.GenerateSolution(SolutionSerializationMode.Bare);
        }

        [TestMethod]
        public void GenerateDay3ChannelSolutionBasedsOnRealData()
        {
            Generator instanceGenerator = new Generator
            {
                DataSource = new StringReader(Properties.Resources.day_DS_D_DH),
                OutputFilename = @"results\day_DS_D_DH_sol.json"
            };
            instanceGenerator.GenerateSolution(SolutionSerializationMode.Bare);
        }

        [TestMethod]
        public void GradeDay3ChannelSolutionFromSavedFiles()
        {
            var file = Properties.Resources.day_DS_D_DH_inst;
            var deserializer = new InstanceJsonSerializer
            {
                Reader = new StreamReader(new MemoryStream(file), Encoding.UTF8)
            };
            Instance instance = deserializer.DeserializeInstance();
            deserializer.Reader = new StreamReader(new MemoryStream(Properties.Resources.day_DS_D_DH_sol), Encoding.UTF8);
            Solution solution = deserializer.DeserializeSolution(instance);
            solution.GradingFunction = new Scorer();
            solution.GradingFunction.AssesSolution(solution);
            InstanceJsonSerializer serializer = new InstanceJsonSerializer()
            {
                Path = @"results\day_DS_D_DH_sol_scored.json"
            };
            serializer.SerializeSolution(solution, SolutionSerializationMode.DebugTaskData);

            Assert.IsNotNull(instance);
            Assert.IsNotNull(solution);
        }

        [TestMethod]
        public void GradeWeek3ChannelSolutionFromSavedFiles()
        {
            var file = Properties.Resources.week_DS_D_DH_inst;
            var deserializer = new InstanceJsonSerializer
            {
                Reader = new StreamReader(new MemoryStream(file), Encoding.UTF8)
            };
            Instance instance = deserializer.DeserializeInstance();
            deserializer.Reader = new StreamReader(new MemoryStream(Properties.Resources.week_DS_D_DH_sol), Encoding.UTF8);
            Solution solution = deserializer.DeserializeSolution(instance);
            solution.GradingFunction = new Scorer();
            solution.GradingFunction.AssesSolution(solution);
            InstanceJsonSerializer serializer = new InstanceJsonSerializer()
            {
                Path = @"results\week_DS_D_DH_sol_scored.json"
            };
            serializer.SerializeSolution(solution, SolutionSerializationMode.DebugTaskData);

            Assert.IsNotNull(instance);
            Assert.IsNotNull(solution);
        }

        [TestMethod]
        public void DeserializeHandmadeEasyInstance()
        {
            var file = Properties.Resources.handmade_extra_easy_inst;
            var deserializer = new InstanceJsonSerializer
            {
                Reader = new StreamReader(new MemoryStream(file), Encoding.UTF8)
            };
            Instance instance = deserializer.DeserializeInstance();
        }

        [TestMethod]
        public void GenerateInstanceJsonSchemaForInstance()
        {
            Generator instanceGenerator = new Generator
            {
                OutputFilename = @"results\instance_schema.json"
            };
            instanceGenerator.GenerateInstanceSchema();
        }

        [TestMethod]
        public void GenerateSolutionJsonSchemaForInstance()
        {
            Generator instanceGenerator = new Generator
            {
                OutputFilename = @"results\solution_schema.json"
            };
            instanceGenerator.GenerateSolutionSchema();
        }
    }
}
