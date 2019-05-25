using InstanceGenerator.InstanceData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGenerator.SolutionObjects
{
    /// <summary>
    /// Intermediate data used for scoring the solution
    /// </summary>
    public class TaskData
    {
        public int ID { get; set; }

        [Description("Overall solution score.")]
        public double WeightedLoss { get; set; }

        [Description("Loss from late ad contract completion.")]
        public double OverdueAdsLoss { get; set; }

        [Description("Loss form scheduling soft-incompatible ads on the same break.")]
        public double MildIncompatibilityLoss { get; set; }

        [Description("Loss form overextending breaks.")]
        public double ExtendedBreakLoss { get; set; }

        public double Wievership { get; set; }

        public int NumberOfStarts { get; set; }
        public int NumberOfEnds { get; set; }
        public int OwnerConflicts { get; set; }
        public int BreakTypeConflicts { get; set; }
        public int SelfSpacingConflicts { get; set; }
        public int SelfIncompatibilityConflicts { get; set; }

        [JsonIgnore]
        public AdvertisementOrder AdvertisementOrderConstraints { get; set; }

        public Dictionary<int, List<int>> BreaksPositions { get; set; } = new Dictionary<int, List<int>>();
    }
}
