using ExampleFileReader;
using ExampleFileReader.InstanceData;
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
                InstanceDataLoader loader = new InstanceDataLoader();
                loader.Reader = reader;
                Instance instance = loader.LoadInstanceFile();
                Assert.IsNotNull(instance);
            }
        }
    }
}
