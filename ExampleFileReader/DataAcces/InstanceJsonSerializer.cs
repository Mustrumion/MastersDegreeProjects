using ExampleFileReader.InstanceData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleFileReader.DataAccess
{
    public class InstanceJsonSerializer
    {
        public string Path { get; set; }
        public TextReader Reader { get; set; }
        public TextWriter Writer { get; set; }

        public void Serialize(Instance instance)
        {
            if (Writer == null)
            {
                Writer = new StreamWriter(Path);
            }
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.Indented,
            };

            JsonSerializer ser = JsonSerializer.Create(settings);
            ser.Serialize(Writer, instance);
            Writer.FlushAsync();
        }

        public Instance Deserialize()
        {
            if(Reader == null)
            {
                Reader = new StreamReader(Path);
            }
            Instance instance = JsonConvert.DeserializeObject<Instance>(Reader.ReadToEnd());
            return instance;
        }
    }
}
