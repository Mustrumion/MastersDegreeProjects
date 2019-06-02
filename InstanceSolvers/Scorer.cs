using InstanceGenerator;
using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers
{
    public class Scorer : IScoringFunction
    {
        public double OverdueTasksLossWeight { get; set; } = 1;
        public double BreakExtensionLossWeight { get; set; } = 1;
        public double MildIncompatibilityLossWeight { get; set; } = 1;

        public Solution Solution { get; set; }
        public Instance Instance { get; set; }
        public string Description
        {
            get =>
$@"Grading function 0.1.0. 
Task delay loss is counted as a task weight multiplied by delay in days (rounded up).
Break extension loss is counted as break extension in seconds.
Soft incompatibility between owners of the same advertisement is a sum of occurece weights multiplied by the corresponding tasks weights.
Weights for the loss function components:
OverdueTasksLossWeight = {OverdueTasksLossWeight}
BreakExtensionLossWeight = {BreakExtensionLossWeight}
MildIncompatibilityLossWeight = {MildIncompatibilityLossWeight}";
        }

        private Dictionary<int, TaskData> _temporaryTaskData;
        private List<int> _currentBreakOrder;
        private TvBreak _currentBreak;
        private int _unitsFromStart;

        private TaskData _currentlyAssessed;
        private int _currentAdId;
        private int _currentAdPosition;
        private AdvertisementOrder _currentAdInfo;
        private int _currentAdCount;
        private Dictionary<int, double> _currentAdIncompatibilityCosts;


        private void CheckAdToAdCompatibility(int otherId, int otherPosition)
        {
            if(_currentAdId == otherId)
            {
                _currentAdCount += 1;
                int distance = Math.Abs(_currentAdPosition - otherPosition);
                if(distance <= _currentAdInfo.MinJobsBetweenSame)
                {
                    _currentlyAssessed.SelfSpacingConflicts += 1;
                }
            }
            else
            {
                AdvertisementOrder otherAdInfo = Instance.AdOrders[otherId];
                if(otherAdInfo.Type == _currentAdInfo.Type)
                {
                    double? incompatibilityWeight = null;
                    if(_currentAdIncompatibilityCosts != null)
                    {
                        bool success = _currentAdIncompatibilityCosts.TryGetValue(otherAdInfo.Brand.ID, out double weight);
                        if (success)
                        {
                            incompatibilityWeight = weight;
                        }
                    }
                    if(incompatibilityWeight != null)
                    {
                        _currentlyAssessed.MildIncompatibilitySumOfOccurenceWeights += incompatibilityWeight.Value;
                    }
                    else
                    {
                        _currentlyAssessed.OwnerConflicts += 1;
                    }
                }
            }
        }


        private void UpdateSimpleStatsForCurrentAd(int position)
        {
            _currentlyAssessed.TimesAired += 1;

            if (position == 0)
            {
                _currentlyAssessed.NumberOfStarts += 1;
            }

            if (position == _currentBreakOrder.Count - 1)
            {
                _currentlyAssessed.NumberOfEnds += 1;
            }

            bool success = Instance.TypeToBreakIncompatibilityMatrix.TryGetValue(_currentAdInfo.Type.ID, out var incompatibleBreaks);
            if (success && incompatibleBreaks.ContainsKey(_currentBreak.ID))
            {
                _currentlyAssessed.BreakTypeConflicts += 1;
            }

            var viewsFunction = _currentBreak.MainViewsFunction;
            if (_currentBreak.TypeViewsFunctions.TryGetValue(_currentAdInfo.Type.ID, out var function))
            {
                viewsFunction = function;
            }

            _currentlyAssessed.Viewership += viewsFunction.GetViewers(_unitsFromStart);

            DateTime adEnd = _currentBreak.StartTime.AddSeconds(Instance.UnitSizeInSeconds *
                (_unitsFromStart + _currentAdInfo.AdSpanUnits));
            if (_currentlyAssessed.LastAdTime == default(DateTime) || adEnd > _currentlyAssessed.LastAdTime)
            {
                _currentlyAssessed.LastAdTime = adEnd;
            }
        }


        private void CalculateAdConstraints(int orderId, int position)
        {
            _currentAdId = orderId;
            _currentAdPosition = position;
            _currentAdCount = 1;
            _currentAdInfo = Instance.AdOrders[_currentAdId];
            bool success = _temporaryTaskData.TryGetValue(_currentAdId, out TaskData assesedTask);
            if (!success)
            {
                assesedTask = new TaskData()
                {
                    AdvertisementOrderData = _currentAdInfo,
                    ScoringFunction = this,
                };
                _temporaryTaskData.Add(_currentAdId, assesedTask);
            }
            _currentlyAssessed = assesedTask;
            Instance.BrandIncompatibilityCost.TryGetValue(_currentAdInfo.Brand.ID, out var currentAdWeights);
            _currentAdIncompatibilityCosts = currentAdWeights;

            UpdateSimpleStatsForCurrentAd(position);

            for (int i = 0; i < _currentBreakOrder.Count; i++)
            {
                if(i != _currentAdPosition)
                {
                    CheckAdToAdCompatibility(_currentBreakOrder[i], i);
                }
            }
            if(_currentAdCount > _currentAdInfo.MaxPerBlock)
            {
                _currentlyAssessed.SelfIncompatibilityConflicts += 1;
            }
        }


        public Dictionary<int, TaskData> AssesBreak(List<int> orderedAds, TvBreak tvBreak)
        {
            _currentBreakOrder = orderedAds;
            _currentBreak = tvBreak;
            _temporaryTaskData = new Dictionary<int, TaskData>();
            _unitsFromStart = 0;
            for(int i = 0; i < orderedAds.Count; i++)
            {
                CalculateAdConstraints(orderedAds[i], i);
                _unitsFromStart += _currentAdInfo.AdSpanUnits;
            }
            return _temporaryTaskData;
        }


        public void AssesSolution(Solution solution)
        {
            Solution = solution;
            Dictionary<int, TaskData> statsData = new Dictionary<int, TaskData>();
            foreach (var tvBreak in Solution.AdvertisementsScheduledOnBreaks)
            {
                Dictionary<int, TaskData> dataToMerge = AssesBreak(tvBreak.Value, Instance.Breaks[tvBreak.Key]);
                foreach(var taskData in dataToMerge)
                {
                    if(statsData.TryGetValue(taskData.Key, out var found))
                    {
                        found.MergeOtherDataIntoThis(taskData.Value);
                    }
                    else
                    {
                        statsData[taskData.Key] = taskData.Value;
                    }
                }
            }
            foreach(var taskData in Solution.AdOrderData)
            {
                if(statsData.TryGetValue(taskData.Key, out var found))
                {
                    taskData.Value.OverwriteStatsWith(found);
                }
                else
                {
                    taskData.Value.RecalculateLoss();
                }
            }
            RecalculateSolutionScoresBasedOnTaskData(solution);
        }


        public void RecalculateSolutionScoresBasedOnTaskData(Solution solution)
        {
            solution.Completion = solution.AdOrderData.Values.Count(v => v.Completed);
            solution.WeightedLoss = solution.AdOrderData.Values.Sum(v => v.WeightedLoss);
            solution.IntegrityLossScore = solution.AdOrderData.Values.Sum(v => v.IntegrityLossScore);
            solution.ExtendedBreakLoss = solution.AdOrderData.Values.Sum(v => v.ExtendedBreakLoss);
            solution.MildIncompatibilityLoss = solution.AdOrderData.Values.Sum(v => v.MildIncompatibilityLoss);
            solution.OverdueAdsLoss = solution.AdOrderData.Values.Sum(v => v.OverdueAdsLoss);
        }


        public void RecalculateOverdueLoss(TaskData taskData)
        {
            var timeDifference = taskData.LastAdTime - taskData.AdvertisementOrderData.DueTime;
            if (timeDifference.TotalMilliseconds > 0)
            {
                taskData.OverdueAdsLoss = Math.Ceiling(timeDifference.TotalDays) * taskData.AdvertisementOrderData.Gain / 10;
            }
            else
            {
                taskData.OverdueAdsLoss = 0;
            }
        }

        public void RecalculateMildIncompatibilityLoss(TaskData taskData)
        {
            taskData.MildIncompatibilityLoss = taskData.MildIncompatibilitySumOfOccurenceWeights * taskData.AdvertisementOrderData.Gain;
        }

        public void RecalculateExtendedBreakLoss(TaskData taskData)
        {
            taskData.ExtendedBreakLoss = taskData.ExtendedBreakSeconds;
        }

        public void RecalculateWeightedLoss(TaskData taskData)
        {
            taskData.WeightedLoss = taskData.ExtendedBreakLoss * BreakExtensionLossWeight 
                + taskData.OverdueAdsLoss * OverdueTasksLossWeight 
                + taskData.MildIncompatibilityLoss * MildIncompatibilityLossWeight;
        }

        public void RecalculateIntegrityLoss(TaskData taskData)
        {
            taskData.IntegrityLossScore =
                1 - taskData.StartsCompletion
                + 1 - taskData.EndsCompletion
                + 1 - taskData.ViewsCompletion
                + 1 - taskData.TimesAiredCompletion
                + taskData.SelfIncompatibilityConflictsProportion
                + taskData.SelfSpacingConflictsProportion
                + taskData.OwnerConflictsProportion
                + taskData.BreakTypeConflictsProportion;
        }

        public void RecalculateLastAdTime(TaskData taskData)
        {
            foreach(var tvBreak in taskData.BreaksPositions)
            {
                var position = tvBreak.Value.Max();
                // TODO: finish this
            }
        }
    }
}
