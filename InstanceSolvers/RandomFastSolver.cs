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
    public class RandomFastSolver : ISolver
    {
        private Instance _instance;
        private Random _random;
        private int _seed;
        private IScoringFunction _scoringFunction;
        private Solution _solution;

        public string Description { get; set; }
        private List<AdvertisementTask> _order { get; set; }

        public RandomFastSolver()
        {
            Random rnd = new Random();
            _seed = rnd.Next();
            _random = new Random(_seed);
        }

        public int Seed
        {
            get => _seed;
            set
            {
                _random = new Random(value);
                _seed = value;
            }
        }

        public Instance Instance
        {
            get
            {
                return _instance;
            }
            set
            {
                _instance = value;
                if (Solution == null)
                {
                    Solution = new Solution(Instance);
                }
            }
        }


        public IScoringFunction ScoringFunction
        {
            get => _scoringFunction;
            set
            {
                _scoringFunction = value;
                if (_scoringFunction.Instance == null)
                {
                    _scoringFunction.Instance = Instance;
                }
                if (_scoringFunction.Solution != _solution)
                {
                    _scoringFunction.Solution = _solution;
                }
                if (Solution != null && Solution.GradingFunction != ScoringFunction)
                {
                    Solution.GradingFunction = ScoringFunction;
                }
            }
        }


        public Solution Solution
        {
            get => _solution;
            set
            {
                _solution = value;
                if (_scoringFunction != null && _scoringFunction.Solution != _solution)
                {
                    _scoringFunction.Solution = _solution;
                }
            }
        }

        public void Solve()
        {
            _order = new List<AdvertisementTask>();
            foreach(var adInfo in Instance.AdOrders.Values)
            {
                for(int i = 0; i< adInfo.MinTimesAired; i++)
                {
                    _order.Add(adInfo);
                }
            }
            _order.Shuffle(_random);
            int curr = 0;
            foreach (var schedule in Solution.AdvertisementsScheduledOnBreaks.Values)
            {
                int currentSize = 0;
                while (curr < _order.Count)
                {
                    if ((currentSize += _order[curr].AdSpanUnits) >= schedule.BreakData.SpanUnits)
                    {
                        break;
                    }
                    schedule.AddAd(_order[curr]);
                    curr++;
                }
            }
            Solution.RestoreTaskView();
            ScoringFunction.AssesSolution(Solution);
        }
    }
}
