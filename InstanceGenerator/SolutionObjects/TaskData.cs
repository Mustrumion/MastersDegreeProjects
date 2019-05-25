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
    /// Intermediate, helper data used for scoring the solutions
    /// </summary>
    public class TaskData
    {
        public int ID { get; set; }

        public double Vievership { get; set; }
        public int TimesAired { get; set; }
        public int NumberOfStarts { get; set; }
        public int NumberOfEnds { get; set; }

        /// <summary>
        /// Overall solution score.
        /// </summary>
        public double WeightedLoss { get; set; }

        /// <summary>
        /// Loss from late ad contract completion.
        /// </summary>
        public double OverdueAdsLoss { get; set; }
        public DateTime LastAdTime { get; set; }

        /// <summary>
        /// Loss form scheduling soft-incompatible ads on the same break.
        /// </summary>
        public double MildIncompatibilityLoss { get; set; }

        /// <summary>
        /// Loss form overextending breaks.
        /// </summary>
        public double ExtendedBreakLoss { get; set; }
        public long ExtendedBreakSeconds { get; set; }

        public int OwnerConflicts { get; set; }
        public int BreakTypeConflicts { get; set; }
        public int SelfSpacingConflicts { get; set; }
        public int SelfIncompatibilityConflicts { get; set; }

        [JsonIgnore]
        public AdvertisementOrder AdvertisementOrderConstraints { get; set; }

        public Dictionary<int, List<int>> BreaksPositions { get; set; } = new Dictionary<int, List<int>>();

        public double StartsProportionUnderMin
        {
            get
            {
                return Math.Max(AdvertisementOrderConstraints.MinBeginingsProportion - (double)NumberOfStarts / TimesAired, 0);
            }
        }

        public bool StartsSatisfied
        {
            get
            {
                return StartsProportionUnderMin <= 0;
            }
        }

        public double EndsProportionUnderMin
        {
            get
            {
                return Math.Max(AdvertisementOrderConstraints.MinEndsProportion - (double)NumberOfEnds / TimesAired, 0);
            }
        }

        public bool EndsSatisfied
        {
            get
            {
                return EndsProportionUnderMin <= 0;
            }
        }
    }
}
