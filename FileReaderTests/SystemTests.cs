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
        public void ReaderReads3ChannelWeeklyFileTest()
        {
            var file = Properties.Resources._3_channel_week;
            using (var reader = new StringReader(file))
            {
                RealInstanceDataLoader loader = new RealInstanceDataLoader();
                loader.Reader = reader;
                Instance instance = loader.LoadInstanceFile();
                Assert.IsNotNull(instance);
            }
        }


        [TestMethod]
        public void SerializeAnInstance()
        {
            var file = Properties.Resources._3_channel_week;
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
            Serializer serializer = new Serializer();
            serializer.Serialize(instance, @"C:\test\text.xml");
        }
    }
}
