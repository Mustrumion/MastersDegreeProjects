using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using InstanceSolvers.MoveFactories;
using InstanceSolvers.Moves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers
{
    public class GreedyHeuristicSolver : ISolver
    {
        private Instance _instance;
        private IScoringFunction _scoringFunction;
        private Solution _solution;
        private bool _movePerformed;
        private int _seed;

        public Random Random { get; set; }
        public int PositionsPerBreakTakenIntoConsideration { get; set; } = 0;
        public int MaxBreakExtensionUnits { get; set; } = 20;

        private List<TvBreak> _breakInOrder { get; set; }
        public string Description { get; set; }

        public GreedyHeuristicSolver()
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
            ScoringFunction.AssesSolution(Solution);
            //due earlier are scheduled first
            //heftier are scheduled first if due at the same time
            var ordersInOrder = Instance.AdOrders.Values.OrderByDescending(order => order.AdSpanUnits).OrderBy(order => order.DueTime).ToList();
            _breakInOrder = Instance.Breaks.Values.OrderBy(b => b.StartTime).ToList();

            _movePerformed = true;
            while (Solution.CompletionScore < 1 && _movePerformed)
            {
                _movePerformed = false;
                foreach (AdvertisementTask order in ordersInOrder)
                {
                    TryToScheduleOrder(order);
                }
            }
        }


        private void ChooseMoveToPerform(List<IMove> moves)
        {
            foreach(var move in moves)
            {
                move.Asses();
            }
            var candidate = moves.OrderBy(m => m.OverallDifference.IntegrityLossScore).FirstOrDefault();
            if(candidate.OverallDifference.IntegrityLossScore < 0)
            {
                candidate.Execute();
                Solution.GradingFunction.RecalculateSolutionScoresBasedOnTaskData(Solution);
                _movePerformed = true;
            }
        }


        private void TryToScheduleOrder(AdvertisementTask order)
        {
            foreach(var tvBreak in _breakInOrder)
            {
                var schedule = Solution.AdvertisementsScheduledOnBreaks[tvBreak.ID];
                if(schedule.UnitFill - MaxBreakExtensionUnits > tvBreak.SpanUnits)
                {
                    continue;
                }
                InsertMoveFactory factory = new InsertMoveFactory(Solution)
                {
                    Breaks = new[] { tvBreak },
                    Tasks = new[] { order },
                    MildlyRandomOrder = false,
                    PositionsCountLimit = PositionsPerBreakTakenIntoConsideration,
                    Random = Random,
                };
                List<IMove> moves = factory.GenerateMoves().ToList();
                ChooseMoveToPerform(moves);
            }
        }
    }
}
