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
    public class GradingFunction : IScoringFunction
    {
        public Solution Solution { get; set; }
        public Instance Instance { get; set; }
        public string Description { get; set; } = "Grading function 0.0.2";

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
                        _currentlyAssessed.MildIncompatibilityLoss += incompatibilityWeight.Value * _currentAdInfo.Gain;
                    }
                    else
                    {
                        _currentlyAssessed.OwnerConflicts += 1;
                    }
                }
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
                assesedTask = new TaskData() { ID = _currentAdId };
                _temporaryTaskData.Add(_currentAdId, assesedTask);
            }
            _currentlyAssessed = assesedTask;
            Instance.BrandIncompatibilityCost.TryGetValue(_currentAdInfo.Brand.ID, out var currentAdWeights);
            _currentAdIncompatibilityCosts = currentAdWeights;
            _currentlyAssessed.TimesAired += 1;

            if(position == 0)
            {
                _currentlyAssessed.NumberOfStarts += 1;
            }

            if(position == _currentBreakOrder.Count)
            {
                _currentlyAssessed.NumberOfEnds += 1;
            }

            success = Instance.TypeToBreakIncompatibilityMatrix.TryGetValue(_currentAdInfo.Type.ID, out var incompatibleBreaks);
            if (success && incompatibleBreaks.ContainsKey(_currentBreak.ID))
            {
                _currentlyAssessed.BreakTypeConflicts += 1;
            }

            var viewsFunction = _currentBreak.MainViewsFunction;
            if(_currentBreak.TypeViewsFunctions.TryGetValue(_currentAdInfo.Type.ID, out var function))
            {
                viewsFunction = function;
            }

            _currentlyAssessed.Vievership += viewsFunction.GetViewers(_unitsFromStart);

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


        public void AssesSolution()
        {
            throw new NotImplementedException();
        }
    }
}
