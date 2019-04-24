using ExampleFileReader;
using ExampleFileReader.InstanceData;
using ExampleFileReader.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileReaderTests
{
    [TestClass]
    public class SystemTests
    {
        [TestMethod]
        public void ReaderReadsWeekly3ChannelFileTest()
        {
            var file = Properties.Resources.week_3channel;
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
        public void SerializeDay3ChannelsInstance()
        {
            var file = Properties.Resources.day_3channels;
            Instance instance = null;
            using (var reader = new StringReader(file))
            {
                RealInstanceDataLoader loader = new RealInstanceDataLoader();
                loader.Reader = reader;
                instance = loader.LoadInstanceFile();
                Assert.IsNotNull(instance);
            }
            SolutionToProblemConverter converter = new SolutionToProblemConverter()
            {
                Instance = instance,
            };
            converter.CreateBreaks();
            InstanceJsonSerializer serializer = new InstanceJsonSerializer();
            serializer.Path = @"C:\test\day_3channels_json.txt";
            serializer.Serialize(instance);
        }


        [TestMethod]
        public void SerializeWeek3ChannelsInstance()
        {
            var file = Properties.Resources.week_3channel;
            Instance instance = null;
            using (var reader = new StringReader(file))
            {
                RealInstanceDataLoader loader = new RealInstanceDataLoader();
                loader.Reader = reader;
                instance = loader.LoadInstanceFile();
                Assert.IsNotNull(instance);
            }
            SolutionToProblemConverter converter = new SolutionToProblemConverter()
            {
                Instance = instance,
            };
            converter.CreateBreaks();
            InstanceJsonSerializer serializer = new InstanceJsonSerializer();
            serializer.Path = @"C:\test\week_3channels_json.txt";
            serializer.Serialize(instance);
        }
    }
}
