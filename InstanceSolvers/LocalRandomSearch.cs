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
    public class LocalRandomSearch : ISolver
    {
        private Instance _instance;
        private IScoringFunction _scoringFunction;
        private Solution _solution;
        private bool _movePerformed;
        private int _seed;
        public Random Random { get; set; }

        public IEnumerable<IMoveFactory> MoveFactories { get; set; }

        private List<TvBreak> _breakInOrder { get; set; }
        public string Description { get; set; }

        public LocalRandomSearch()
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
            CreateMoveFactoriesIfEmpty();
            ScoringFunction.AssesSolution(Solution);
            _movePerformed = true;
            while (_movePerformed)
            {
                _movePerformed = false;
                List<IMove> moves = new List<IMove>();
                foreach(IMoveFactory factory in MoveFactories)
                {
                    moves.AddRange(factory.GenerateMoves().ToList());
                }
                ChooseMoveToPerform(moves);
            }
        }

        private void CreateMoveFactoriesIfEmpty()
        {
            if (MoveFactories == null)
            {
                MoveFactories = new List<IMoveFactory>
                {
                    new InsertMoveFactory(Solution)
                    {
                        MildlyRandomOrder = true,
                        PositionsCountLimit = 10,
                        MaxTasksChecked = 5,
                        MaxBreaksChecked = 10,
                        IgnoreWhenUnitOverfillAbove = 10,
                        IgnoreTasksWithCompletedViews = true,
                        Random = Random,
                    },
                    new DeleteMoveFactory(Solution)
                    {
                        MildlyRandomOrder = true,
                        PositionsCountLimit = 10,
                        MaxBreaksChecked = 10,
                        Random = Random,
                    },
                    new SwapMoveFactory(Solution)
                    {
                        MildlyRandomOrder = true,
                        PositionsCountLimit = 10,
                        MaxTasksChecked = 5,
                        MaxBreaksChecked = 10,
                        Random = Random,
                    },
                };
            }
        }

        private void ChooseMoveToPerform(List<IMove> moves)
        {
            if(moves.Count == 0)
            {
                return;
            }
            foreach (var move in moves)
            {
                move.Asses();
            }
            var candidate = moves.OrderBy(m => m.OverallDifference.WeightedLoss).OrderBy(m => m.OverallDifference.IntegrityLossScore).FirstOrDefault();
            if (candidate.OverallDifference.IntegrityLossScore < 0 || (candidate.OverallDifference.IntegrityLossScore == 0 && candidate.OverallDifference.WeightedLoss < 0))
            {
                candidate.Execute();
                Solution.GradingFunction.RecalculateSolutionScoresBasedOnTaskData(Solution);
                _movePerformed = true;
            }
        }
    }
}
