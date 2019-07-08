using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGenerator.SolutionObjects
{
    public class TasksStats
    {
        public int NumberOfTasks { get; set; }

        public double Viewership { get; set; }
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
        public double MildIncompatibilitySumOfOccurenceWeights { get; set; }

        /// <summary>
        /// Loss form overextending breaks.
        /// </summary>
        public double ExtendedBreakLoss { get; set; }
        public long ExtendedBreakSeconds { get; set; }

        public int OwnerConflicts { get; set; }
        public int BreakTypeConflicts { get; set; }
        public int SelfSpacingConflicts { get; set; }
        public int SelfIncompatibilityConflicts { get; set; }

        public double IntegrityLossScore { get; set; }


        public double StartsCompletion { get; set; }
        public double EndsCompletion { get; set; }
        public double ViewsCompletion { get; set; }
        public double TimesAiredCompletion { get; set; }
        public int TaskCompletion { get; set; }

        public double OwnerConflictsProportion { get; set; }
        public double BreakTypeConflictsProportion { get; set; }
        public double SelfSpacingConflictsProportion { get; set; }
        public double SelfIncompatibilityConflictsProportion { get; set; }
    }
}
