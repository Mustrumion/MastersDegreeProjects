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
    public class FastGreedyHeuristic : ISolver
    {
        private Instance _instance;
        private Random _random;
        private int _seed;
        private IScoringFunction _scoringFunction;
        private Solution _solution;

        public string Description { get; set; }
        public int MaxOverfillUnits { get; set; } = 10;
        private List<AdvertisementTask> _order { get; set; }

        public FastGreedyHeuristic()
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
            Solution.RestoreTaskView();
            var breakList = Solution.AdvertisementsScheduledOnBreaks.Values.ToList();
            breakList.Shuffle(_random);
            foreach (var schedule in breakList)
            {
                CreateSchedule(schedule);
            }
            ScoringFunction.RecalculateSolutionScoresBasedOnTaskData(Solution);
        }

        private void AddToSolutionScores(Dictionary<int, TaskData> addedScores)
        {
            foreach (var taskData in addedScores.Values)
            {
                TaskData currentStatsForTask = Solution.AdOrderData[taskData.TaskID];
                currentStatsForTask.MergeOtherDataIntoThis(taskData);
            }
        }


        private void CreateSchedule(BreakSchedule schedule)
        {
            var advertisementDataList = Solution.AdOrderData.Values.Where(t => !t.TimesAiredSatisfied || t.ViewsSatisfied).ToList();
            advertisementDataList.Shuffle(_random);
            foreach(var ad in advertisementDataList)
            {
                if (Instance.TypeToBreakIncompatibilityMatrix.TryGetValue(ad.AdvertisementOrderData.ID, out var incompatibleBreaks))
                {
                    if (incompatibleBreaks.ContainsKey(schedule.BreakData.ID))
                    {
                        continue;
                    }
                }
                Instance.BrandIncompatibilityCost.TryGetValue(ad.AdvertisementOrderData.Type.ID, out var brandCompatibility);
                if(schedule.Order.Any(a => a.Type.ID == ad.AdvertisementOrderData.Type.ID && (brandCompatibility == null || !brandCompatibility.ContainsKey(a.Brand.ID))))
                {
                    continue;
                }
                schedule.Append(ad.AdvertisementOrderData);
                if(schedule.UnitFill > schedule.BreakData.SpanUnits + MaxOverfillUnits)
                {
                    break;
                }
            }
            ScoringFunction.AssesBreak(schedule);
            AddToSolutionScores(schedule.Scores);
        }
    }
}
