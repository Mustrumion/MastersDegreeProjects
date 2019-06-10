using InstanceGenerator;
using InstanceGenerator.DataAccess;
using InstanceGenerator.InstanceData;
using InstanceGenerator.InstanceModification;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using InstanceSolvers;
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
            ISolver solver = new RandomSolver()
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
