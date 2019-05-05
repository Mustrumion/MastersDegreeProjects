using InstanceGenerator;
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
            var file = Properties.Resources.week_3channels_json;
            var reader = new InstanceJsonSerializer();
            reader.Reader = new StringReader(file);
            Instance instance = reader.Deserialize();
            Assert.IsNotNull(instance);
        }


        [TestMethod]
        public void GenerateDay3ChannelsInstanceBasedOnRealData()
        {
            var file = Properties.Resources.day_DS_D_DH;
            Instance instance = null;
            using (var reader = new StringReader(file))
            {
                RealInstanceDataLoader loader = new RealInstanceDataLoader();
                loader.Reader = reader;
                instance = loader.LoadInstanceFile();
                Assert.IsNotNull(instance);
            }
            RealInstanceToProblemConverter converter = new RealInstanceToProblemConverter()
            {
                Instance = instance,
                TimeUnitInSeconds = 3.0,
            };
            converter.ConvertToProblem();
            InstanceJsonSerializer serializer = new InstanceJsonSerializer();
            serializer.Path = @"day_3channels_json.txt";
            serializer.Serialize(instance);
        }


        [TestMethod]
        public void GenerateWeek3ChannelsInstanceBasedOnRealData()
        {
            var file = Properties.Resources.week_DS_D_DH;
            Instance instance = null;
            using (var reader = new StringReader(file))
            {
                RealInstanceDataLoader loader = new RealInstanceDataLoader();
                loader.Reader = reader;
                instance = loader.LoadInstanceFile();
                Assert.IsNotNull(instance);
            }
            RealInstanceToProblemConverter converter = new RealInstanceToProblemConverter()
            {
                Instance = instance,
                TimeUnitInSeconds = 3.0,
            };
            converter.ConvertToProblem();
            InstanceJsonSerializer serializer = new InstanceJsonSerializer();
            serializer.Path = @"week_3channels_json.txt";
            serializer.Serialize(instance);
        }


        [TestMethod]
        public void GenerateHour3ChannelsInstanceBasedOnRealData()
        {
            var file = Properties.Resources.hour_DS_D_DH;
            Instance instance = null;
            using (var reader = new StringReader(file))
            {
                RealInstanceDataLoader loader = new RealInstanceDataLoader();
                loader.Reader = reader;
                instance = loader.LoadInstanceFile();
                Assert.IsNotNull(instance);
            }
            RealInstanceToProblemConverter converter = new RealInstanceToProblemConverter()
            {
                Instance = instance,
                TimeUnitInSeconds = 3.0,
            };
            converter.ConvertToProblem();
            InstanceJsonSerializer serializer = new InstanceJsonSerializer();
            serializer.Path = @"hour_3channels_json.txt";
            serializer.Serialize(instance);
        }


        [TestMethod]
        public void GenerateMonth3ChannelsInstanceBasedOnRealData()
        {
            var file = Properties.Resources.month_DS_D_DH;
            Instance instance = null;
            using (var reader = new StringReader(file))
            {
                RealInstanceDataLoader loader = new RealInstanceDataLoader();
                loader.Reader = reader;
                instance = loader.LoadInstanceFile();
                Assert.IsNotNull(instance);
            }
            RealInstanceToProblemConverter converter = new RealInstanceToProblemConverter()
            {
                Instance = instance,
                TimeUnitInSeconds = 3.0,
            };
            converter.ConvertToProblem();
            InstanceJsonSerializer serializer = new InstanceJsonSerializer();
            serializer.Path = @"month_3channels_json.txt";
            serializer.Serialize(instance);
        }

        [TestMethod]
        public void GenerateJsonSchemaForInstance()
        {
            JSchemaGenerator generator = new JSchemaGenerator();

            generator.GenerationProviders.Add(new StringEnumGenerationProvider());
            generator.DefaultRequired = Required.Default;
            generator.SchemaLocationHandling = SchemaLocationHandling.Inline;
            generator.SchemaReferenceHandling = SchemaReferenceHandling.All;
            generator.SchemaIdGenerationHandling = SchemaIdGenerationHandling.FullTypeName;

            JSchema schema = generator.Generate(typeof(Instance));
            string json = schema.ToString();
            StreamWriter writer = new StreamWriter(@"instance_json_schema.json");
            writer.Write(json);
            writer.FlushAsync();
        }
    }
}
