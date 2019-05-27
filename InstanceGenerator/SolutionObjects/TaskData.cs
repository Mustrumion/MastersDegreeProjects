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
        public int TaskID { get; set; }

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

        public IScoringFunction ScoringFunction { get; set; }

        [JsonIgnore]
        public AdvertisementOrder AdvertisementOrderData { get; set; }

        public Dictionary<int, List<int>> BreaksPositions { get; set; } = new Dictionary<int, List<int>>();


        #region CalculatedProperties

        public double StartsProportion
        {
            get => (double)NumberOfStarts / TimesAired;
        }
        public double StartsCompletion
        {
            get => Math.Min(StartsProportion / AdvertisementOrderData.MinBeginingsProportion, 1);
        }
        public bool StartsSatisfied
        {
            get => AdvertisementOrderData.MinBeginingsProportion <= StartsProportion;
        }


        public double EndsProportion
        {
            get => (double)NumberOfEnds / TimesAired;
        }
        public double EndsCompletion
        {
            get => Math.Min(EndsProportion / AdvertisementOrderData.MinEndsProportion, 1);
        }
        public bool EndsSatisfied
        {
            get => AdvertisementOrderData.MinEndsProportion <= EndsProportion;
        }


        public double ViewsCompletion
        {
            get => Math.Min(Viewership / AdvertisementOrderData.MinViewership, 1);
        }
        public bool ViewsSatisfied
        {
            get => Viewership >= AdvertisementOrderData.MinViewership;
        }


        public double TimesAiredCompletion
        {
            get => Math.Min((double)TimesAired / AdvertisementOrderData.MinTimesAired, 1);
        }
        public bool TimesAiredSatisfied
        {
            get => TimesAired >= AdvertisementOrderData.MinTimesAired;
        }


        public double OwnerConflictsProportion
        {
            get => (double)OwnerConflicts / AdvertisementOrderData.MinTimesAired;
        }
        public double BreakTypeConflictsProportion
        {
            get => (double)BreakTypeConflicts / AdvertisementOrderData.MinTimesAired;
        }
        public double SelfSpacingConflictsProportion
        {
            get => (double)SelfSpacingConflicts / AdvertisementOrderData.MinTimesAired;
        }
        public double SelfIncompatibilityConflictsProportion
        {
            get => (double)SelfIncompatibilityConflicts / AdvertisementOrderData.MinTimesAired;
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


        public void MergeOtherDataIntoThis(TaskData taskData)
        {
            ExtendedBreakSeconds += taskData.ExtendedBreakSeconds;
            Viewership += taskData.Viewership;
            TimesAired += taskData.TimesAired;
            NumberOfEnds += taskData.NumberOfEnds;
            NumberOfStarts += taskData.NumberOfStarts;
            if (taskData.LastAdTime > LastAdTime)
            {
                LastAdTime = taskData.LastAdTime;
            }

            MildIncompatibilityLoss += taskData.MildIncompatibilityLoss;
            ExtendedBreakSeconds += taskData.ExtendedBreakSeconds;
            ScoringFunction.RecalculateOverdueLoss(this);
            ScoringFunction.RecalculateMildIncompatibilityLoss(this);
            ScoringFunction.RecalculateExtendedBreakLoss(this);

            ScoringFunction.RecalculateWeightedLoss(this);
            ScoringFunction.RecalculateIntegrityLoss(this);

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
        }
    }
}
