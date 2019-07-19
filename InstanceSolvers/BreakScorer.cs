using InstanceGenerator.InstanceData;
using InstanceGenerator.SolutionObjects;
using InstanceGenerator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers
{
    public class BreakScorer
    {
        public Instance Instance { get; set; }
        public IScoringFunction ScoringFunction { get; set; }

        private Dictionary<int, TaskScore> _taksAssessments;
        private TvBreak _breakData;
        private int _unitsFromStart;

        private BreakSchedule _schedule;
        private TaskScore _currentlyAssessed;
        private int _currentAdPosition;
        private AdvertisementTask _currentAd;
        private int _currentAdCount;
        private Dictionary<int, double> _currentAdIncompatibilityCosts;


        private void CheckAdToAdCompatibility(AdvertisementTask other, int otherPosition)
        {
            if (_currentAd == other && otherPosition != _currentAdPosition)
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
                    ScoringFunction = ScoringFunction,
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
    }
}
