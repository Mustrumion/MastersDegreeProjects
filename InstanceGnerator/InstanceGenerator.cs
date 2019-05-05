using InstanceGenerator.DataAccess;
using InstanceGenerator.InstanceModification;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGenerator
{
    public class InstanceGenerator
    {
        public RealInstanceDataLoader InstanceLoader { get; set; } = new RealInstanceDataLoader();
        public StreamReader DataSource { get; set; }
        public RealInstanceToProblemConverter InstanceCoverter { get; set; } = new RealInstanceToProblemConverter();

        public void GenerateInstance()
        {
            RealInstanceDataLoader loader = new RealInstanceDataLoader();
            loader.Reader = DataSource;
            var instance = loader.LoadInstanceFile();
            InstanceCoverter.Instance = instance;
            InstanceCoverter.ConvertToProblem();
            InstanceJsonSerializer serializer = new InstanceJsonSerializer();
            serializer.Path = @"result.txt";
            serializer.Serialize(instance);
        }
    }
}
