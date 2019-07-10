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


        public void AddTaskData(TaskScore taskData)
        {
            NumberOfTasks += 1;

            Viewership += taskData.Viewership;
            TimesAired += taskData.TimesAired;
            NumberOfEnds += taskData.NumberOfEnds;
            NumberOfStarts += taskData.NumberOfStarts;

            if (taskData.LastAdTime > LastAdTime)
            {
                LastAdTime = taskData.LastAdTime;
            }
            ExtendedBreakSeconds += taskData.ExtendedBreakSeconds;

            OwnerConflicts += taskData.OwnerConflicts;
            BreakTypeConflicts += taskData.BreakTypeConflicts;
            SelfSpacingConflicts += taskData.SelfSpacingConflicts;
            SelfIncompatibilityConflicts += taskData.SelfIncompatibilityConflicts;

            WeightedLoss += taskData.WeightedLoss;
            OverdueAdsLoss += taskData.OverdueAdsLoss;
            MildIncompatibilityLoss += taskData.MildIncompatibilityLoss;
            IntegrityLossScore += taskData.IntegrityLossScore;
            ExtendedBreakLoss += taskData.ExtendedBreakLoss;

            StartsCompletion += taskData.StartsCompletion;
            EndsCompletion += taskData.EndsCompletion;
            ViewsCompletion += taskData.ViewsCompletion;
            TimesAiredCompletion += taskData.TimesAiredCompletion;
            TaskCompletion += taskData.Completed ? 1 : 0;

            OwnerConflictsProportion += taskData.OwnerConflictsProportion;
            BreakTypeConflictsProportion += taskData.BreakTypeConflictsProportion;
            SelfSpacingConflictsProportion += taskData.SelfSpacingConflictsProportion;
            SelfIncompatibilityConflictsProportion += taskData.SelfIncompatibilityConflictsProportion;
        }

        public void AddTasksStats(TasksStats taskData)
        {
            NumberOfTasks += taskData.NumberOfTasks;

            Viewership += taskData.Viewership;
            TimesAired += taskData.TimesAired;
            NumberOfEnds += taskData.NumberOfEnds;
            NumberOfStarts += taskData.NumberOfStarts;

            if (taskData.LastAdTime > LastAdTime)
            {
                LastAdTime = taskData.LastAdTime;
            }
            ExtendedBreakSeconds += taskData.ExtendedBreakSeconds;

            OwnerConflicts += taskData.OwnerConflicts;
            BreakTypeConflicts += taskData.BreakTypeConflicts;
            SelfSpacingConflicts += taskData.SelfSpacingConflicts;
            SelfIncompatibilityConflicts += taskData.SelfIncompatibilityConflicts;

            WeightedLoss += taskData.WeightedLoss;
            OverdueAdsLoss += taskData.OverdueAdsLoss;
            MildIncompatibilityLoss += taskData.MildIncompatibilityLoss;
            IntegrityLossScore += taskData.IntegrityLossScore;
            ExtendedBreakLoss += taskData.ExtendedBreakLoss;

            StartsCompletion += taskData.StartsCompletion;
            EndsCompletion += taskData.EndsCompletion;
            ViewsCompletion += taskData.ViewsCompletion;
            TimesAiredCompletion += taskData.TimesAiredCompletion;
            TaskCompletion += taskData.TaskCompletion;

            OwnerConflictsProportion += taskData.OwnerConflictsProportion;
            BreakTypeConflictsProportion += taskData.BreakTypeConflictsProportion;
            SelfSpacingConflictsProportion += taskData.SelfSpacingConflictsProportion;
            SelfIncompatibilityConflictsProportion += taskData.SelfIncompatibilityConflictsProportion;
        }
    }
}
