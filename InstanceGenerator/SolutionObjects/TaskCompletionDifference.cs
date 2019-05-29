using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGenerator.SolutionObjects
{
    public class TaskCompletionDifference
    {
        public double Viewership { get; set; }
        public int TimesAired { get; set; }
        public int NumberOfStarts { get; set; }
        public int NumberOfEnds { get; set; }
        public double WeightedLoss { get; set; }
        public double OverdueAdsLoss { get; set; }
        public TimeSpan LastAdTime { get; set; }
        public double MildIncompatibilityLoss { get; set; }
        public double MildIncompatibilitySumOfOccurenceWeights { get; set; }
        public double ExtendedBreakLoss { get; set; }
        public long ExtendedBreakSeconds { get; set; }
        public int OwnerConflicts { get; set; }
        public int BreakTypeConflicts { get; set; }
        public int SelfSpacingConflicts { get; set; }
        public int SelfIncompatibilityConflicts { get; set; }
    }
}
