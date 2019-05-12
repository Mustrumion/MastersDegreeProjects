﻿using InstanceGenerator;
using InstanceGenerator.DataAccess;
using InstanceGenerator.InstanceData;
using InstanceGenerator.InstanceModification;
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
                RealInstanceDataLoader loader = new RealInstanceDataLoader();
                loader.Reader = reader;
                Instance instance = loader.LoadInstanceFile();
                Assert.IsNotNull(instance);
            }
        }

        [TestMethod]
        public void DeserializeWeek3ChannelTest()
        {
            var file = Properties.Resources.week_DS_D_DH_inst;
            var reader = new InstanceJsonSerializer();
            reader.Reader = new StreamReader(new MemoryStream(file), Encoding.UTF8);
            Instance instance = reader.Deserialize();
            Assert.IsNotNull(instance);
        }


        [TestMethod]
        public void GenerateDay3ChannelsInstanceBasedOnRealData()
        {
            Generator instanceGenerator = new Generator();
            instanceGenerator.DataSource = new StringReader(Properties.Resources.day_DS_D_DH);
            instanceGenerator.OutputFilename = @"results\day_DS_D_DH_inst.json";
            instanceGenerator.GenerateInstance();
        }


        [TestMethod]
        public void GenerateWeek3ChannelsInstanceBasedOnRealData()
        {
            Generator instanceGenerator = new Generator();
            instanceGenerator.DataSource = new StringReader(Properties.Resources.week_DS_D_DH);
            instanceGenerator.OutputFilename = @"results\week_DS_D_DH_inst.json";
            instanceGenerator.GenerateInstance();
        }


        [TestMethod]
        public void GenerateHour3ChannelsInstanceBasedOnRealData()
        {
            Generator instanceGenerator = new Generator();
            instanceGenerator.DataSource = new StringReader(Properties.Resources.hour_DS_D_DH);
            instanceGenerator.OutputFilename = @"results\hour_DS_D_DH_inst.json";
            instanceGenerator.GenerateInstance();
        }


        [TestMethod]
        public void GenerateMonth3ChannelsInstanceBasedOnRealData()
        {
            Generator instanceGenerator = new Generator();
            instanceGenerator.DataSource = new StringReader(Properties.Resources.month_DS_D_DH);
            instanceGenerator.OutputFilename = @"results\month_DS_D_DH_inst.json";
            instanceGenerator.GenerateInstance();
        }

        [TestMethod]
        public void GenerateMonth3ChannelSolutionBasenOnRealData()
        {
            Generator instanceGenerator = new Generator();
            instanceGenerator.DataSource = new StringReader(Properties.Resources.month_DS_D_DH);
            instanceGenerator.OutputFilename = @"results\month_DS_D_DH_sol.json";
            instanceGenerator.GenerateSolution();
        }

        [TestMethod]
        public void GenerateInstanceJsonSchemaForInstance()
        {
            Generator instanceGenerator = new Generator();
            instanceGenerator.OutputFilename = @"results\instance_schema.json";
            instanceGenerator.GenerateInstanceSchema();
        }

        [TestMethod]
        public void GenerateSolutionJsonSchemaForInstance()
        {
            Generator instanceGenerator = new Generator();
            instanceGenerator.OutputFilename = @"results\solution_schema.json";
            instanceGenerator.GenerateSolutionSchema();
        }
    }
}
