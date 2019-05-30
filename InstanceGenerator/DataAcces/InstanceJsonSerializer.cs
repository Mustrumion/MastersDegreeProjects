using InstanceGenerator.DataAcces;
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
    public enum SolutionSerializationMode
    {
        /// <summary>
        /// No stats nor loss data will be included. Only the pure schedule order data. Files will not be indented by default.
        /// </summary>
        Bare = 1,
        /// <summary>
        /// Bare + information about solution scores. Files will not be indented by default.
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

        public void SerializeSolution(Solution solution, SolutionSerializationMode solutionSerializationMode = SolutionSerializationMode.Basic)
        {
            if (Writer == null)
            {
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
                jsonResolver.IgnoreProperty(typeof(TaskData), nameof(TaskData.BreaksPositions));
            }
            if (solutionSerializationMode == SolutionSerializationMode.Basic)
            {
                jsonResolver.IgnoreProperty(typeof(Solution), nameof(Solution.AdOrderData));
            }
            if (solutionSerializationMode == SolutionSerializationMode.Bare)
            {
                jsonResolver.IgnoreProperty(typeof(Solution), 
                    nameof(Solution.AdOrderData), 
                    nameof(Solution.Completion),
                    nameof(Solution.CompletionScore),
                    nameof(Solution.MaxCompletion),
                    nameof(Solution.MildIncompatibilityLoss),
                    nameof(Solution.OverdueAdsLoss),
                    nameof(Solution.ExtendedBreakLoss),
                    nameof(Solution.WeightedLoss),
                    nameof(Solution.IntegrityLossScore),
                    nameof(Solution.GradingFunctionDescription));
            }

            JsonSerializer ser = JsonSerializer.Create(settings);
            ser.ContractResolver = jsonResolver;
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
            instance.RestoreStructuresAfterDeserialization();
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
