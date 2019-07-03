using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using InstanceSolvers.MoveFactories;
using InstanceSolvers.Moves;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers
{
    public class LocalSearchSolver : BaseSolver, ISolver
    {
        private bool _movePerformed;
        public Stopwatch Stopwatch { get; set; } = new Stopwatch();

        public IEnumerable<IMoveFactory> MoveFactories { get; set; }

        private List<TvBreak> _breakInOrder { get; set; }
        public string Description { get; set; }

        public bool StopWhenCompleted { get; set; } = true;
        public bool StopWhenStepScoreDecrease { get; set; }
        public TimeSpan MaxTime { get; set; }
        public bool PropagateRandomSeed { get; set; }

        public ISolver InitialSolver { get; set; }

        public LocalSearchSolver() : base()
        {
        }

        public void Solve()
        {
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
            if(InitialSolver != null)
            {
                InitialSolver.Instance = Instance;
                if (PropagateRandomSeed)
                {
                    InitialSolver.Seed = Random.Next();
                }
                InitialSolver.ScoringFunction = ScoringFunction;
                InitialSolver.Solve();
                Solution = InitialSolver.Solution;
            }
            InsertionHeuristic insertionHeuristic = new InsertionHeuristic()
            {
                MaxBreakExtensionUnits = 30,
            };
            insertionHeuristic.Instance = Instance;
            insertionHeuristic.ScoringFunction = ScoringFunction;
            insertionHeuristic.Solution = Solution;
            insertionHeuristic.Solve();
            Solution = insertionHeuristic.Solution;

            InitializeMoveFactories();
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
            Stopwatch.Stop();
            Solution.TimeElapsed += Stopwatch.Elapsed;
        }

        private void InitializeMoveFactories()
        {
            if (MoveFactories == null)
            {
                MoveFactories = new List<IMoveFactory>
                {
                    new InsertMoveFactory()
                    {
                        MildlyRandomOrder = true,
                        PositionsCountLimit = 5,
                        MaxTasksChecked = 5,
                        MaxBreaksChecked = 5,
                        IgnoreWhenUnitOverfillAbove = 60,
                        IgnoreCompletedTasks = true,
                        IgnoreTasksWithCompletedViews = false,
                    },
                    new InsertMoveFactory()
                    {
                        MildlyRandomOrder = true,
                        PositionsCountLimit = 5,
                        MaxTasksChecked = 5,
                        MaxBreaksChecked = 5,
                        IgnoreWhenUnitOverfillAbove = 60,
                        IgnoreTasksWithCompletedViews = false,
                    },
                    new DeleteMoveFactory()
                    {
                        MildlyRandomOrder = true,
                        PositionsCountLimit = 10,
                        MaxBreaksChecked = 10,
                    },
                    new SwapMoveFactory()
                    {
                        MildlyRandomOrder = true,
                        PositionsCountLimit = 10,
                        MaxTasksChecked = 5,
                        MaxBreaksChecked = 5,
                    },
                };
            }
            foreach (var moveFactory in MoveFactories)
            {
                if (PropagateRandomSeed)
                {
                    moveFactory.Random = new Random(Random.Next());
                }
                moveFactory.Solution = Solution;
            }
        }

        private bool TimeToEnd()
        {
            if(Solution.CompletionScore >= 1 && StopWhenCompleted)
            {
                return true;
            }
            if(MaxTime != default(TimeSpan) && Stopwatch.Elapsed > MaxTime)
            {
                return true;
            }
            return false;
        }

        private void ChooseMoveToPerform(List<IMove> moves)
        {
            if(moves.Count == 0 || TimeToEnd())
            {
                return;
            }
            foreach (var move in moves)
            {
                if (TimeToEnd())
                {
                    return;
                }
                move.Asses();
            }
            var candidate = moves.OrderBy(m => m.OverallDifference.WeightedLoss).OrderBy(m => m.OverallDifference.IntegrityLossScore).FirstOrDefault();
            if (!candidate.OverallDifference.HasScoreWorsened() || !StopWhenStepScoreDecrease)
            {
                candidate.Execute();
                Solution.GradingFunction.RecalculateSolutionScoresBasedOnTaskData(Solution);
                _movePerformed = true;
            }
        }
    }
}
