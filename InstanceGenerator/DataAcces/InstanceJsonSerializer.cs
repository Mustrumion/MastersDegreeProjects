using InstanceGenerator.InstanceData;
using InstanceGenerator.SolutionObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGenerator.DataAccess
{
    public class InstanceJsonSerializer
    {
        public string Path { get; set; }
        public TextReader Reader { get; set; }
        public TextWriter Writer { get; set; }

        public void SerializeInstance(Instance instance)
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

        public void SerializeSolution(Solution solution)
        {
            if (Writer == null)
            {
                Writer = new StreamWriter(Path);
            }
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                Formatting = Formatting.Indented,
            };

            JsonSerializer ser = JsonSerializer.Create(settings);
            ser.Serialize(Writer, solution);
            Writer.FlushAsync();
        }

        public Instance DeserializeInstance()
        {
            if (Reader == null)
            {
                Reader = new StreamReader(Path);
            }
            Instance instance = JsonConvert.DeserializeObject<Instance>(Reader.ReadToEnd());
            return instance;
        }

        public Solution DeserializeSolution(Instance instance = null)
        {
            if (Reader == null)
            {
                Reader = new StreamReader(Path);
            }
            Solution solution = JsonConvert.DeserializeObject<Solution>(Reader.ReadToEnd());
            if(instance != null)
            {
                solution.Instance = instance;
                solution.RestoreHelperStructures();
            }
            return solution;
        }
    }
}
