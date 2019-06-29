using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGenerator.SolutionObjects
{
    public class TaskCompletionDifference
    {
        public double WeightedLoss { get; set; }
        public double OverdueAdsLoss { get; set; }
        public double MildIncompatibilityLoss { get; set; }
        public double ExtendedBreakLoss { get; set; }
        public double IntegrityLossScore { get; set; }

        public double EndsCompletion { get; set; }
        public double StartsCompletion { get; set; }
        public double ViewsCompletion { get; set; }
        public double TimesAiredCompletion { get; set; }

        public double SelfIncompatibilityConflictsProportion { get; set; }
        public double SelfSpacingConflictsProportion { get; set; }
        public double OwnerConflictsProportion { get; set; }
        public double BreakTypeConflictsProportion { get; set; }

        public void Add(TaskCompletionDifference other)
        {
            WeightedLoss += other.WeightedLoss;
            OverdueAdsLoss += other.OverdueAdsLoss;
            MildIncompatibilityLoss += other.MildIncompatibilityLoss;
            ExtendedBreakLoss += other.ExtendedBreakLoss;
            IntegrityLossScore += other.IntegrityLossScore;
            EndsCompletion += other.EndsCompletion;
            StartsCompletion += other.StartsCompletion;
            ViewsCompletion += other.ViewsCompletion;
            TimesAiredCompletion += other.TimesAiredCompletion;
            SelfIncompatibilityConflictsProportion += other.SelfIncompatibilityConflictsProportion;
            SelfSpacingConflictsProportion += other.SelfSpacingConflictsProportion;
            OwnerConflictsProportion += other.OwnerConflictsProportion;
            BreakTypeConflictsProportion += other.BreakTypeConflictsProportion;
        }

        public bool HasScoreWorsened()
        {
            return IntegrityLossScore > 0 || (IntegrityLossScore == 0 && WeightedLoss > 0);
        }
    }
}
