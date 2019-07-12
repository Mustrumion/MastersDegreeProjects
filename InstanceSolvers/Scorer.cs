using InstanceGenerator;
using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using Newtonsoft.Json;
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

        private Solution _solution;
        [JsonIgnore]
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

        private Dictionary<int, TaskScore> _taksAssessments;
        private TvBreak _breakData;
        private int _unitsFromStart;

        private BreakSchedule _schedule;
        private TaskScore _currentlyAssessed;
        private int _currentAdPosition;
        private AdvertisementTask _currentAd;
        private int _currentAdCount;
        private Dictionary<int, double> _currentAdIncompatibilityCosts;

        [JsonIgnore]
        public Solution Solution
        {
            get => _solution;
            set
            {
                if(_solution == value || value == null)
                {
                    return;
                }
                _solution = value;
                _solution.GradingFunction = this;
            }
        }

        private void CheckAdToAdCompatibility(AdvertisementTask other, int otherPosition)
        {
            if (_currentAd == other)
            {
                _currentAdCount += 1;
                int distance = Math.Abs(_currentAdPosition - otherPosition);
                if (distance <= _currentAd.MinJobsBetweenSame)
                {
                    _currentlyAssessed.SelfSpacingConflicts += 1;
                }
            }
            else
            {
                if (other.Type.ID == _currentAd.Type.ID && other.Brand.ID != _currentAd.Brand.ID)
                {
                    if (_currentAdIncompatibilityCosts != null)
                    {
                        if (_currentAdIncompatibilityCosts.TryGetValue(other.Brand.ID, out double weight))
                        {
                            _currentlyAssessed.MildIncompatibilitySumOfOccurenceWeights += weight;
                        }
                        else
                        {
                            _currentlyAssessed.OwnerConflicts += 1;
                        }
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

            if (position == _schedule.Count - 1)
            {
                _currentlyAssessed.NumberOfEnds += 1;
            }

            bool success = Instance.TypeToBreakIncompatibilityMatrix.TryGetValue(_currentAd.Type.ID, out var incompatibleBreaks);
            if (success && incompatibleBreaks.ContainsKey(_breakData.ID))
            {
                _currentlyAssessed.BreakTypeConflicts += 1;
            }

            var viewsFunction = _breakData.MainViewsFunction;
            if (_breakData.TypeViewsFunctions.TryGetValue(_currentAd.Type.ID, out var function))
            {
                viewsFunction = function;
            }

            int adEndUnit = _schedule.GetPositionEndUnits(position);
            if (adEndUnit > _breakData.SpanUnits)
            {
                _currentlyAssessed.ExtendedBreakUnits = Math.Min(adEndUnit - _breakData.SpanUnits, _currentAd.AdSpanUnits);
            }

            _currentlyAssessed.Viewership += viewsFunction.GetViewers(_unitsFromStart);

            DateTime adEnd = _breakData.StartTime.AddSeconds(Instance.UnitSizeInSeconds *
                (_unitsFromStart + _currentAd.AdSpanUnits));
            if (_currentlyAssessed.LastAdTime == default(DateTime) || adEnd > _currentlyAssessed.LastAdTime)
            {
                _currentlyAssessed.LastAdTime = adEnd;
            }
        }


        private void CalculateAdConstraints(AdvertisementTask order, int position)
        {
            _currentAdPosition = position;
            _currentAdCount = 1;
            _currentAd = order;
            if (!_taksAssessments.TryGetValue(order.ID, out TaskScore assesedTask))
            {
                assesedTask = new TaskScore()
                {
                    AdConstraints = _currentAd,
                    ScoringFunction = this,
                };
                assesedTask.BreaksPositions.Add(_breakData.ID, new List<int>());
                _taksAssessments.Add(order.ID, assesedTask);
            }
            assesedTask.BreaksPositions[_breakData.ID].Add(position);
            _currentlyAssessed = assesedTask;
            Instance.BrandIncompatibilityCost.TryGetValue(_currentAd.Brand.ID, out var currentAdWeights);
            _currentAdIncompatibilityCosts = currentAdWeights;

            UpdateSimpleStatsForCurrentAd(position);

            for (int i = 0; i < _schedule.Count; i++)
            {
                if (i != _currentAdPosition)
                {
                    CheckAdToAdCompatibility(_schedule.Order[i], i);
                }
            }
            if (_currentAdCount > _currentAd.MaxPerBlock)
            {
                _currentlyAssessed.SelfIncompatibilityConflicts += 1;
            }
        }


        public void AssesBreak(BreakSchedule schedule)
        {
            _schedule = schedule;
            _breakData = schedule.BreakData;
            _taksAssessments = new Dictionary<int, TaskScore>();
            _unitsFromStart = 0;
            for (int i = 0; i < _schedule.Count; i++)
            {
                CalculateAdConstraints(_schedule.Order[i], i);
                _unitsFromStart += _currentAd.AdSpanUnits;
            }
            schedule.Scores = _taksAssessments;
        }


        public void AssesSolution(Solution solution)
        {
            Solution = solution;
            Dictionary<int, TaskScore> statsData = Instance.AdOrders.ToDictionary(
                    a => a.Key,
                    a => new TaskScore() { AdConstraints = a.Value, ScoringFunction = this });
            foreach (var taskData in statsData.Values)
            {
                taskData.RecalculateLoss();
            }
            foreach (var tvBreak in Solution.AdvertisementsScheduledOnBreaks.Values)
            {
                if (tvBreak.Scores == null)
                {
                    AssesBreak(tvBreak);
                }
                Dictionary<int, TaskScore> dataToMerge = tvBreak.Scores;
                foreach (var taskData in dataToMerge)
                {
                    statsData[taskData.Key].MergeOtherDataIntoThis(taskData.Value);
                }
            }
            solution.AdOrdersScores = statsData;
            RecalculateSolutionScoresBasedOnTaskData(solution);
        }


        public void RecalculateSolutionScoresBasedOnTaskData(Solution solution)
        {
            solution.Completion = 0;
            solution.WeightedLoss = 0;
            solution.IntegrityLossScore = 0;
            solution.ExtendedBreakLoss = 0;
            solution.MildIncompatibilityLoss = 0;
            solution.OverdueAdsLoss = 0;
            solution.TotalStats = new TasksStats();
            foreach (var taskScore in solution.AdOrdersScores.Values)
            {
                solution.Completion += taskScore.Completed ? 1 : 0;
                solution.WeightedLoss += taskScore.WeightedLoss;
                solution.IntegrityLossScore += taskScore.IntegrityLossScore;
                solution.ExtendedBreakLoss += taskScore.ExtendedBreakLoss;
                solution.MildIncompatibilityLoss += taskScore.MildIncompatibilityLoss;
                solution.OverdueAdsLoss += taskScore.OverdueAdsLoss;
                solution.TotalStats.AddTaskData(taskScore);
            }
            solution.Scored = true;
        }


        public void RecalculateOverdueLoss(TaskScore taskData)
        {
            var timeDifference = taskData.LastAdTime - taskData.AdConstraints.DueTime;
            if (timeDifference.TotalMilliseconds > 0)
            {
                taskData.OverdueAdsLoss = Math.Ceiling(timeDifference.TotalDays) * taskData.AdConstraints.Gain / 10;
            }
            else
            {
                taskData.OverdueAdsLoss = 0;
            }
        }

        public void RecalculateMildIncompatibilityLoss(TaskScore taskData)
        {
            taskData.MildIncompatibilityLoss = taskData.MildIncompatibilitySumOfOccurenceWeights * taskData.AdConstraints.Gain;
        }

        public void RecalculateExtendedBreakLoss(TaskScore taskData)
        {
            taskData.ExtendedBreakLoss = taskData.ExtendedBreakUnits;
        }

        public void RecalculateWeightedLoss(TaskScore taskData)
        {
            taskData.WeightedLoss = taskData.ExtendedBreakLoss * BreakExtensionLossWeight
                + taskData.OverdueAdsLoss * OverdueTasksLossWeight
                + taskData.MildIncompatibilityLoss * MildIncompatibilityLossWeight;
        }

        public void RecalculateIntegrityLoss(TaskScore taskData)
        {
            taskData.IntegrityLossScore =
                1 - taskData.StartsCompletion
                + (taskData.StartsSatisfied ? 0 : 0.01)
                + 1 - taskData.EndsCompletion
                + (taskData.EndsSatisfied ? 0 : 0.01)
                + 1 - taskData.ViewsCompletion
                + (taskData.ViewsSatisfied ? 0 : 0.01)
                + 1 - taskData.TimesAiredCompletion
                + (taskData.TimesAiredSatisfied ? 0 : 0.01)
                + taskData.SelfIncompatibilityConflictsProportion
                + taskData.SelfSpacingConflictsProportion
                + taskData.OwnerConflictsProportion
                + taskData.BreakTypeConflictsProportion;
        }

        public void RecalculateLastAdTime(TaskScore taskData)
        {
            DateTime currentMax = DateTime.MinValue;
            foreach (var tvBreak in taskData.BreaksPositions)
            {
                int position = tvBreak.Value.Max();
                BreakSchedule breakSchedule = Solution.AdvertisementsScheduledOnBreaks[tvBreak.Key];
                DateTime time = breakSchedule.BreakData.StartTime.AddSeconds(breakSchedule.GetPositionEndUnits(position) * Instance.UnitSizeInSeconds);
                if(time > currentMax)
                {
                    currentMax = time;
                }
            }
            taskData.LastAdTime = currentMax;
        }
    }
}
