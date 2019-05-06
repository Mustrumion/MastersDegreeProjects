using InstanceGenerator.DataAccess;
using InstanceGenerator.InstanceData;
using InstanceGenerator.InstanceModification;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGenerator
{
    public class Generator
    {
        public RealInstanceDataLoader InstanceLoader { get; set; } = new RealInstanceDataLoader();
        /// <summary>
        /// Stream reader for the real data slice
        /// </summary>
        public TextReader DataSource { get; set; }
        /// <summary>
        /// The filepath is chacked if DataSource is not set
        /// </summary>
        public string SourcePath { get; set; }
        public string OutputFilename { get; set; }
        public RealInstanceToProblemConverter InstanceCoverter { get; set; } = new RealInstanceToProblemConverter();

        public void GenerateInstance()
        {
            if (DataSource == null)
            {
                DataSource = new StreamReader(SourcePath);
            }
            InstanceLoader.Reader = DataSource;
            var instance = InstanceLoader.LoadInstanceFile();
            InstanceCoverter.Instance = instance;
            InstanceCoverter.ConvertToProblem();
            InstanceJsonSerializer serializer = new InstanceJsonSerializer();
            if(OutputFilename == null)
            {
                OutputFilename = Path.GetFileNameWithoutExtension(SourcePath) + ".json";
            }
            serializer.Path = OutputFilename;
            serializer.Serialize(instance);
        }

        public void GenerateSchema()
        {
            JSchemaGenerator generator = new JSchemaGenerator();

            generator.GenerationProviders.Add(new StringEnumGenerationProvider());
            generator.DefaultRequired = Required.Default;
            generator.SchemaLocationHandling = SchemaLocationHandling.Inline;
            generator.SchemaReferenceHandling = SchemaReferenceHandling.All;
            generator.SchemaIdGenerationHandling = SchemaIdGenerationHandling.FullTypeName;

            JSchema schema = generator.Generate(typeof(Instance));
            string json = schema.ToString();
            StreamWriter writer = new StreamWriter(OutputFilename);
            writer.Write(json);
            writer.FlushAsync();
        }
    }
}
