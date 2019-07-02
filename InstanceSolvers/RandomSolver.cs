using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using InstanceSolvers.Moves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers
{
    public class RandomSolver : ISolver
    {
        private Instance _instance;
        private int _seed;
        private IScoringFunction _scoringFunction;
        private Solution _solution;

        public Random Random { get; set; }
        public string Description { get; set; }

        public RandomSolver()
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
                if(_scoringFunction != null && _scoringFunction.Solution != _solution)
                {
                    _scoringFunction.Solution = _solution;
                }
            }
        }


        private bool InsertInRandomNonFilledBreak(TaskData taskData)
        {
            List<BreakSchedule> breaksWithEnoughSpace = Solution.AdvertisementsScheduledOnBreaks.Values.Where
                (
                    b => b.BreakData.SpanUnits >= b.UnitFill + taskData.AdvertisementOrderData.AdSpanUnits
                ).ToList();
            if (breaksWithEnoughSpace.Count == 0)
            {
                return false;
            }
            int breakNum = Random.Next(breaksWithEnoughSpace.Count);
            BreakSchedule schedule = breaksWithEnoughSpace[breakNum];
            int position = Random.Next(schedule.Count + 1);
            Insert insert = new Insert()
            {
                TvBreak = schedule.BreakData,
                AdvertisementOrder = taskData.AdvertisementOrderData,
                Position = position,
                Instance = Instance,
                Solution = Solution,
            };
            insert.Execute();
            return true;
        }

        private void InsertInRandomBreak(TaskData taskData)
        {
            int breakNum = Random.Next(Instance.Breaks.Count);
            TvBreak tvBreak = Instance.Breaks.Values.ToList()[breakNum];
            BreakSchedule schedule = Solution.AdvertisementsScheduledOnBreaks[tvBreak.ID];
            int position = Random.Next(schedule.Count + 1);
            Insert insert = new Insert()
            {
                TvBreak = schedule.BreakData,
                AdvertisementOrder = taskData.AdvertisementOrderData,
                Position = position,
                Instance = Instance,
                Solution = Solution,
            };
            insert.Execute();
        }

        public void Solve()
        {
            ScoringFunction.AssesSolution(Solution);
            while (Solution.AdOrderData.Values.Any(a => !a.TimesAiredSatisfied))
            {
                foreach (AdvertisementTask advertisementOrder in Instance.AdOrders.Values)
                {
                    var taskData = Solution.AdOrderData[advertisementOrder.ID];
                    if (taskData.TimesAiredSatisfied)
                    {
                        continue;
                    }
                    if (!InsertInRandomNonFilledBreak(taskData))
                    {
                        InsertInRandomBreak(taskData);
                    }
                }
            }
            ScoringFunction.RecalculateSolutionScoresBasedOnTaskData(Solution);
        }
    }
}
