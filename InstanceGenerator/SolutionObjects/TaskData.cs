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
    public class TaskData
    {
        public int TaskID { get => AdvertisementOrderData.ID; }

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

        [JsonIgnore]
        public IScoringFunction ScoringFunction { get; set; }

        [JsonIgnore]
        public AdvertisementOrder AdvertisementOrderData { get; set; }

        [JsonProperty(Order = 1)]
        public Dictionary<int, List<int>> BreaksPositions { get; set; } = new Dictionary<int, List<int>>();


        #region CalculatedProperties

        public double StartsProportion
        {
            get
            {
                if(TimesAired == 0) return 0;
                return (double)NumberOfStarts / Math.Max(TimesAired, AdvertisementOrderData.MinTimesAired);
            }
        }
        public double StartsCompletion
        {
            get
            {
                if (AdvertisementOrderData.MinBeginingsProportion == 0) return 1;
                return Math.Min(StartsProportion / AdvertisementOrderData.MinBeginingsProportion, 1);
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
                return (double)NumberOfEnds / Math.Max(TimesAired, AdvertisementOrderData.MinTimesAired);
            }
        }
        public double EndsCompletion
        {
            get
            {
                if (AdvertisementOrderData.MinEndsProportion == 0) return 1;
                return Math.Min(EndsProportion / AdvertisementOrderData.MinEndsProportion, 1);
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
                if (AdvertisementOrderData.MinViewership == 0) return 1;
                return Math.Min(Viewership / AdvertisementOrderData.MinViewership, 1);
            }
        }
        public bool ViewsSatisfied
        {
            get => Viewership >= AdvertisementOrderData.MinViewership;
        }


        public double TimesAiredCompletion
        {
            get
            {
                if (AdvertisementOrderData.MinTimesAired == 0) return 1;
                return Math.Min((double)TimesAired / AdvertisementOrderData.MinTimesAired, 1);
            }
        }
        public bool TimesAiredSatisfied
        {
            get => TimesAired >= AdvertisementOrderData.MinTimesAired;
        }


        public double OwnerConflictsProportion
        {
            get => (double)OwnerConflicts / Math.Max(AdvertisementOrderData.MinTimesAired, Math.Max(TimesAired, 1));
        }
        public double BreakTypeConflictsProportion
        {
            get => (double)BreakTypeConflicts / Math.Max(AdvertisementOrderData.MinTimesAired, Math.Max(TimesAired, 1));
        }
        public double SelfSpacingConflictsProportion
        {
            get => (double)SelfSpacingConflicts / Math.Max(AdvertisementOrderData.MinTimesAired, Math.Max(TimesAired, 1));
        }
        public double SelfIncompatibilityConflictsProportion
        {
            get => (double)SelfIncompatibilityConflicts / Math.Max(AdvertisementOrderData.MinTimesAired, Math.Max(TimesAired, 1));
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

        public void MergeOtherDataIntoThis(TaskData taskData)
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
            ExtendedBreakSeconds += taskData.ExtendedBreakSeconds;

            OwnerConflicts += taskData.OwnerConflicts;
            BreakTypeConflicts += taskData.BreakTypeConflicts;
            SelfSpacingConflicts += taskData.SelfSpacingConflicts;
            SelfIncompatibilityConflicts += taskData.SelfIncompatibilityConflicts;

            foreach (var tvBreak in taskData.BreaksPositions)
            {
                if (BreaksPositions.TryGetValue(tvBreak.Key, out var list))
                {
                    list.AddRange(list);
                }
                else
                {
                    BreaksPositions.Add(tvBreak.Key, tvBreak.Value);
                }
            }
            RecalculateLoss();
        }


        public void RemoveOtherDataFromThis(TaskData taskData)
        {
            Viewership -= taskData.Viewership;
            TimesAired -= taskData.TimesAired;
            NumberOfEnds -= taskData.NumberOfEnds;
            NumberOfStarts -= taskData.NumberOfStarts;

            MildIncompatibilitySumOfOccurenceWeights -= taskData.MildIncompatibilitySumOfOccurenceWeights;
            ExtendedBreakSeconds -= taskData.ExtendedBreakSeconds;

            OwnerConflicts -= taskData.OwnerConflicts;
            BreakTypeConflicts -= taskData.BreakTypeConflicts;
            SelfSpacingConflicts -= taskData.SelfSpacingConflicts;
            SelfIncompatibilityConflicts -= taskData.SelfIncompatibilityConflicts;

            foreach (var tvBreak in taskData.BreaksPositions)
            {
                if (BreaksPositions.TryGetValue(tvBreak.Key, out var list))
                {
                    list = list.Except(tvBreak.Value).ToList();
                    if(list.Count == 0)
                    {
                        BreaksPositions.Remove(tvBreak.Key);
                    }
                    else
                    {
                        BreaksPositions[tvBreak.Key] = list;
                    }
                }
                else
                {
                    throw new Exception("Something went wrong. This data was not a part of this task.");
                }
            }
            if (taskData.LastAdTime == LastAdTime)
            {
                RecalculateLastAdTime();
            }
            RecalculateLoss();
        }


        private void RecalculateLastAdTime()
        {
            ScoringFunction.RecalculateLastAdTime(this);
        }


        public void OverwriteStatsWith(TaskData taskData)
        {
            Viewership = taskData.Viewership;
            TimesAired = taskData.TimesAired;
            NumberOfEnds = taskData.NumberOfEnds;
            NumberOfStarts = taskData.NumberOfStarts;
            LastAdTime = taskData.LastAdTime;
            MildIncompatibilitySumOfOccurenceWeights = taskData.MildIncompatibilitySumOfOccurenceWeights;
            ExtendedBreakSeconds = taskData.ExtendedBreakSeconds;
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

        /// <summary>
        /// Creates a deep clone of TaskData
        /// </summary>
        /// <returns></returns>
        public TaskData Clone()
        {
            TaskData clone = new TaskData()
            {
                Viewership = Viewership,
                TimesAired = TimesAired,
                NumberOfEnds = NumberOfEnds,
                NumberOfStarts = NumberOfStarts,
                LastAdTime = LastAdTime,
                MildIncompatibilitySumOfOccurenceWeights = MildIncompatibilitySumOfOccurenceWeights,
                ExtendedBreakSeconds = ExtendedBreakSeconds,
                OwnerConflicts = OwnerConflicts,
                BreakTypeConflicts = BreakTypeConflicts,
                SelfSpacingConflicts = SelfSpacingConflicts,
                SelfIncompatibilityConflicts = SelfIncompatibilityConflicts,
                ExtendedBreakLoss = ExtendedBreakLoss,
                MildIncompatibilityLoss = MildIncompatibilityLoss,
                OverdueAdsLoss = OverdueAdsLoss,
                IntegrityLossScore = IntegrityLossScore,
                WeightedLoss = WeightedLoss,
                AdvertisementOrderData = AdvertisementOrderData,
                ScoringFunction = ScoringFunction,
                BreaksPositions = BreaksPositions.ToDictionary(b => b.Key, b => b.Value.ToList()),
            };
            return clone;
        }

        public TaskCompletionDifference CalculateDifference(TaskData taskData)
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
