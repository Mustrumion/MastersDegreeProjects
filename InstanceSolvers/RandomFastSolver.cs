using InstanceGenerator;
using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers
{
    public class RandomFastSolver : ISolver
    {
        private Instance _instance;
        private int _seed;
        private IScoringFunction _scoringFunction;
        private Solution _solution;

        public Random Random { get; set; }
        public string Description { get; set; }
        private List<AdvertisementTask> _order { get; set; }

        public RandomFastSolver()
        {
            Random rnd = new Random();
            _seed = rnd.Next();
            Random = new Random(_seed);
        }

        public int Seed
        {
            get => _seed;
            set
            {
                Random = new Random(value);
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
                if (Solution == null || Solution.Instance != Instance)
                {
                    Solution = new Solution(Instance);
                }
                if (ScoringFunction != null && ScoringFunction.Instance != Instance)
                {
                    ScoringFunction.Instance = Instance;
                }
            }
        }


        public IScoringFunction ScoringFunction
        {
            get => _scoringFunction;
            set
            {
                _scoringFunction = value;
                if (_scoringFunction.Instance != Instance)
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
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            _order = new List<AdvertisementTask>();
            foreach(var adInfo in Instance.AdOrders.Values)
            {
                for(int i = 0; i< adInfo.MinTimesAired; i++)
                {
                    _order.Add(adInfo);
                }
            }
            _order.Shuffle(Random);
            int curr = 0;
            var breakList = Solution.AdvertisementsScheduledOnBreaks.Values.ToList();
            breakList.Shuffle(Random);
            foreach (var schedule in breakList)
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
            stopwatch.Stop();
            Solution.TimeElapsed += stopwatch.Elapsed;
        }
    }
}
