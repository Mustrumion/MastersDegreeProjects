using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
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
    public class TaskScore
    {
        public int ID { get => AdConstraints.ID; }

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
        public long ExtendedBreakUnits { get; set; }

        public int OwnerConflicts { get; set; }
        public int BreakTypeConflicts { get; set; }
        public int SelfSpacingConflicts { get; set; }
        public int SelfIncompatibilityConflicts { get; set; }

        public double IntegrityLossScore { get; set; }

        [JsonIgnore]
        public IScoringFunction ScoringFunction { get; set; }

        [JsonIgnore]
        public AdvertisementTask AdConstraints { get; set; }

        #region CalculatedProperties

        public double StartsProportion
        {
            get
            {
                if(TimesAired == 0) return 0;
                return (double)NumberOfStarts / Math.Max(TimesAired, AdConstraints.MinTimesAired);
            }
        }
        public double StartsCompletion
        {
            get
            {
                if (AdConstraints.MinBeginingsProportion == 0) return 1;
                return Math.Min(StartsProportion / AdConstraints.MinBeginingsProportion, 1);
            }
        }
        public bool StartsSatisfied
        {
            get => StartsCompletion >= 1;
        }


        public double EndsProportion
        {
            get
            {
                if (TimesAired == 0) return 0;
                return (double)NumberOfEnds / Math.Max(TimesAired, AdConstraints.MinTimesAired);
            }
        }
        public double EndsCompletion
        {
            get
            {
                if (AdConstraints.MinEndsProportion == 0) return 1;
                return Math.Min(EndsProportion / AdConstraints.MinEndsProportion, 1);
            }
        }
        public bool EndsSatisfied
        {
            get => EndsCompletion >= 1;
        }


        public double ViewsCompletion
        {
            get
            {
                if (AdConstraints.MinViewership == 0) return 1;
                return Math.Min(Viewership / AdConstraints.MinViewership, 1);
            }
        }
        public bool ViewsSatisfied
        {
            get => Viewership >= AdConstraints.MinViewership;
        }


        public double TimesAiredCompletion
        {
            get
            {
                if (AdConstraints.MinTimesAired == 0) return 1;
                return Math.Min((double)TimesAired / AdConstraints.MinTimesAired, 1);
            }
        }
        public bool TimesAiredSatisfied
        {
            get => TimesAired >= AdConstraints.MinTimesAired;
        }


        public double OwnerConflictsProportion
        {
            get => (double)OwnerConflicts / Math.Max(AdConstraints.MinTimesAired, Math.Max(TimesAired, 1));
        }
        public double BreakTypeConflictsProportion
        {
            get => (double)BreakTypeConflicts / Math.Max(AdConstraints.MinTimesAired, Math.Max(TimesAired, 1));
        }
        public double SelfSpacingConflictsProportion
        {
            get => (double)SelfSpacingConflicts / Math.Max(AdConstraints.MinTimesAired, Math.Max(TimesAired, 1));
        }
        public double SelfIncompatibilityConflictsProportion
        {
            get => (double)SelfIncompatibilityConflicts / Math.Max(AdConstraints.MinTimesAired, Math.Max(TimesAired, 1));
        }


        public bool Completed
        {
            get
            {
                if (!StartsSatisfied) return false;
                if (!EndsSatisfied) return false;
                if (!ViewsSatisfied) return false;
                if (!TimesAiredSatisfied) return false;
                if (OwnerConflicts > 0) return false;
                if (BreakTypeConflicts > 0) return false;
                if (SelfSpacingConflicts > 0) return false;
                if (SelfIncompatibilityConflicts > 0) return false;
                return true;
            }
        }

        #endregion CalculatedProperties


        public void RecalculateLoss()
        {
            if(ScoringFunction != null)
            {
                ScoringFunction.RecalculateOverdueLoss(this);
                ScoringFunction.RecalculateMildIncompatibilityLoss(this);
                ScoringFunction.RecalculateExtendedBreakLoss(this);

                ScoringFunction.RecalculateWeightedLoss(this);
                ScoringFunction.RecalculateIntegrityLoss(this);
            }
        }

        public void MergeOtherDataIntoThis(TaskScore taskData)
        {
            Viewership += taskData.Viewership;
            TimesAired += taskData.TimesAired;
            NumberOfEnds += taskData.NumberOfEnds;
            NumberOfStarts += taskData.NumberOfStarts;

            if (taskData.LastAdTime > LastAdTime)
            {
                LastAdTime = taskData.LastAdTime;
            }
            MildIncompatibilitySumOfOccurenceWeights += taskData.MildIncompatibilitySumOfOccurenceWeights;
            ExtendedBreakUnits += taskData.ExtendedBreakUnits;

            OwnerConflicts += taskData.OwnerConflicts;
            BreakTypeConflicts += taskData.BreakTypeConflicts;
            SelfSpacingConflicts += taskData.SelfSpacingConflicts;
            SelfIncompatibilityConflicts += taskData.SelfIncompatibilityConflicts;
            
            RecalculateLoss();
        }


        public void RemoveOtherDataFromThis(TaskScore taskData, BreakSchedule tvBreak)
        {
            Viewership -= taskData.Viewership;
            TimesAired -= taskData.TimesAired;
            NumberOfEnds -= taskData.NumberOfEnds;
            NumberOfStarts -= taskData.NumberOfStarts;

            MildIncompatibilitySumOfOccurenceWeights -= taskData.MildIncompatibilitySumOfOccurenceWeights;
            ExtendedBreakUnits -= taskData.ExtendedBreakUnits;

            OwnerConflicts -= taskData.OwnerConflicts;
            BreakTypeConflicts -= taskData.BreakTypeConflicts;
            SelfSpacingConflicts -= taskData.SelfSpacingConflicts;
            SelfIncompatibilityConflicts -= taskData.SelfIncompatibilityConflicts;
            
            if (taskData.LastAdTime == LastAdTime)
            {
                RecalculateLastAdTime(tvBreak);
            }
            RecalculateLoss();
        }


        private void RecalculateLastAdTime(BreakSchedule tvBreak)
        {
            LastAdTime = default(DateTime);
            if (TimesAired == 0)
            {
                return;
            }
            ScoringFunction.RecalculateLastAdTime(this, tvBreak);
        }


        public void OverwriteStatsWith(TaskScore taskData)
        {
            Viewership = taskData.Viewership;
            TimesAired = taskData.TimesAired;
            NumberOfEnds = taskData.NumberOfEnds;
            NumberOfStarts = taskData.NumberOfStarts;
            LastAdTime = taskData.LastAdTime;
            MildIncompatibilitySumOfOccurenceWeights = taskData.MildIncompatibilitySumOfOccurenceWeights;
            ExtendedBreakUnits = taskData.ExtendedBreakUnits;
            OwnerConflicts = taskData.OwnerConflicts;
            BreakTypeConflicts = taskData.BreakTypeConflicts;
            SelfSpacingConflicts = taskData.SelfSpacingConflicts;
            SelfIncompatibilityConflicts = taskData.SelfIncompatibilityConflicts;
            ExtendedBreakLoss = taskData.ExtendedBreakLoss;
            MildIncompatibilityLoss = taskData.MildIncompatibilityLoss;
            OverdueAdsLoss = taskData.OverdueAdsLoss;
            IntegrityLossScore = taskData.IntegrityLossScore;
            WeightedLoss = taskData.WeightedLoss;
        }

        public bool IsStatEqual(TaskScore taskData)
        {
            if (Viewership != taskData.Viewership) return false;
            if (TimesAired != taskData.TimesAired) return false;
            if (NumberOfEnds != taskData.NumberOfEnds) return false;
            if (NumberOfStarts != taskData.NumberOfStarts) return false;
            if (LastAdTime != taskData.LastAdTime) return false;
            if (MildIncompatibilitySumOfOccurenceWeights != taskData.MildIncompatibilitySumOfOccurenceWeights) return false;
            if (ExtendedBreakUnits != taskData.ExtendedBreakUnits) return false;
            if (OwnerConflicts != taskData.OwnerConflicts) return false;
            if (BreakTypeConflicts != taskData.BreakTypeConflicts) return false;
            if (SelfSpacingConflicts != taskData.SelfSpacingConflicts) return false;
            if (SelfIncompatibilityConflicts != taskData.SelfIncompatibilityConflicts) return false;
            if (ExtendedBreakLoss != taskData.ExtendedBreakLoss) return false;
            if (MildIncompatibilityLoss != taskData.MildIncompatibilityLoss) return false;
            if (OverdueAdsLoss != taskData.OverdueAdsLoss) return false;
            if (IntegrityLossScore != taskData.IntegrityLossScore) return false;
            if (WeightedLoss != taskData.WeightedLoss) return false;
            return true;
        }

        /// <summary>
        /// Creates a deep clone of TaskData
        /// </summary>
        /// <returns></returns>
        public TaskScore Clone(bool copyStatsOnly = false)
        {
            TaskScore clone = new TaskScore()
            {
                Viewership = Viewership,
                TimesAired = TimesAired,
                NumberOfEnds = NumberOfEnds,
                NumberOfStarts = NumberOfStarts,
                LastAdTime = LastAdTime,
                MildIncompatibilitySumOfOccurenceWeights = MildIncompatibilitySumOfOccurenceWeights,
                ExtendedBreakUnits = ExtendedBreakUnits,
                OwnerConflicts = OwnerConflicts,
                BreakTypeConflicts = BreakTypeConflicts,
                SelfSpacingConflicts = SelfSpacingConflicts,
                SelfIncompatibilityConflicts = SelfIncompatibilityConflicts,
                ExtendedBreakLoss = ExtendedBreakLoss,
                MildIncompatibilityLoss = MildIncompatibilityLoss,
                OverdueAdsLoss = OverdueAdsLoss,
                IntegrityLossScore = IntegrityLossScore,
                WeightedLoss = WeightedLoss,
                AdConstraints = AdConstraints,
                ScoringFunction = ScoringFunction,
            };
            return clone;
        }

        public TaskCompletionDifference CalculateDifference(TaskScore taskData)
        {
            return new TaskCompletionDifference()
            {
                ExtendedBreakLoss = ExtendedBreakLoss - taskData.ExtendedBreakLoss,
                MildIncompatibilityLoss = MildIncompatibilityLoss - taskData.MildIncompatibilityLoss,
                OverdueAdsLoss = OverdueAdsLoss - taskData.OverdueAdsLoss,
                IntegrityLossScore = IntegrityLossScore - taskData.IntegrityLossScore,
                WeightedLoss = WeightedLoss - taskData.WeightedLoss,
                TimesAiredCompletion = TimesAiredCompletion - taskData.TimesAiredCompletion,
                EndsCompletion = EndsCompletion - taskData.EndsCompletion,
                StartsCompletion = StartsCompletion - taskData.StartsCompletion,
                ViewsCompletion = ViewsCompletion - taskData.ViewsCompletion,
                BreakTypeConflictsProportion = BreakTypeConflictsProportion - taskData.BreakTypeConflictsProportion,
                OwnerConflictsProportion = OwnerConflictsProportion - taskData.OwnerConflictsProportion,
                SelfIncompatibilityConflictsProportion = SelfIncompatibilityConflictsProportion - taskData.SelfIncompatibilityConflictsProportion,
                SelfSpacingConflictsProportion = SelfSpacingConflictsProportion - taskData.SelfSpacingConflictsProportion,
            };
        }
    }
}
