using InstanceGenerator.DataAcces;
using InstanceGenerator.InstanceData;
using InstanceGenerator.SolutionObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InstanceGenerator.DataAccess
{
    public enum SolutionSerializationMode
    {
        /// <summary>
        /// No stats nor loss data will be included. Only the pure schedule order data. Files will not be indented by default.
        /// </summary>
        Bare = 1,
        /// <summary>
        /// Bare + information about solution scores and stats. Files will not be indented by default.
        /// </summary>
        Basic = 2,
        /// <summary>
        /// Basic + intermediate score for each task. Files will be indented by default.
        /// </summary>
        DebugTaskData = 3,
        /// <summary>
        /// TaskData + position of scheduled tasks. Files will be indented by default.
        /// </summary>
        DebugFull = 4,
    }

    public class InstanceJsonSerializer
    {
        public string Path { get; set; }
        public TextReader Reader { get; set; }
        public TextWriter Writer { get; set; }

        public void SerializeInstance(Instance instance)
        {
            if (Writer == null)
            {
                FileInfo file = new FileInfo(Path);
                if (!file.Directory.Exists)
                {
                    file.Directory.Create();
                }
                Writer = new StreamWriter(Path);
            }
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.Indented,
            };

            JsonSerializer ser = JsonSerializer.Create(settings);
            ser.Serialize(Writer, instance);
            Writer.Flush();
            Writer.Close();
            Writer = null;
        }

        public void SerializeSolution(Solution solution, SolutionSerializationMode solutionSerializationMode = SolutionSerializationMode.Basic)
        {
            solution.PrepareForSerialization();
            if (Writer == null)
            {
                FileInfo file = new FileInfo(Path);
                if (!file.Directory.Exists)
                {
                    file.Directory.Create();
                }
                Writer = new StreamWriter(Path);
            }
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                Formatting = (int)solutionSerializationMode > 2 ? Formatting.Indented : Formatting.None,
            };

            var jsonResolver = new PropertyRenameAndIgnoreSerializerContractResolver();
            if (solutionSerializationMode == SolutionSerializationMode.DebugTaskData)
            {
            }
            if (solutionSerializationMode == SolutionSerializationMode.Basic)
            {
                jsonResolver.IgnoreProperty(typeof(Solution), nameof(Solution.AdOrdersScores));
            }
            if (solutionSerializationMode == SolutionSerializationMode.Bare)
            {
                jsonResolver.IgnoreProperty(typeof(Solution), 
                    nameof(Solution.AdOrdersScores), 
                    nameof(Solution.Completion),
                    nameof(Solution.CompletionScore),
                    nameof(Solution.MaxCompletion),
                    nameof(Solution.MildIncompatibilityLoss),
                    nameof(Solution.OverdueAdsLoss),
                    nameof(Solution.ExtendedBreakLoss),
                    nameof(Solution.WeightedLoss),
                    nameof(Solution.IntegrityLossScore),
                    nameof(Solution.TotalStats),
                    nameof(Solution.GradingFunctionDescription));
            }

            JsonSerializer ser = JsonSerializer.Create(settings);
            ser.ContractResolver = jsonResolver;
            ser.Serialize(Writer, solution);
            Writer.Flush();
            Writer.Close();
            Writer = null;
        }

        public Instance DeserializeInstance()
        {
            if (Reader == null)
            {
                Reader = WaitForFile(Path);
            }
            Instance instance;
            var serializer = new JsonSerializer();
            using (var jsonTextReader = new JsonTextReader(Reader))
            {
                instance = serializer.Deserialize<Instance>(jsonTextReader);
            }
            Reader.Close();
            Reader = null;
            instance.RestoreStructuresAfterDeserialization();
            return instance;
        }

        private StreamReader WaitForFile(string fullPath)
        {
            for (int numTries = 0; numTries < 40; numTries++)
            {
                StreamReader fs = null;
                try
                {
                    fs = new StreamReader(fullPath);
                    return fs;
                }
                catch (IOException)
                {
                    if (fs != null)
                    {
                        fs.Dispose();
                    }
                    Thread.Sleep(50);
                }
            }
            return null;
        }

        public Solution DeserializeSolution(Instance instance = null)
        {
            if (Reader == null)
            {
                Reader = WaitForFile(Path);
            }
            Solution solution;
            var serializer = new JsonSerializer();
            using (var jsonTextReader = new JsonTextReader(Reader))
            {
                solution = serializer.Deserialize<Solution>(jsonTextReader);
            }
            Reader.Close();
            Reader = null;
            
            if(instance != null)
            {
                solution.Instance = instance;
                solution.RestoreStructures();
            }
            return solution;
        }
    }
}
