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
    public enum Action
    {
        Perform = 0,
        Ignore = 1,
        Stop = 2,
    }

    public class LocalSearchSolver : BaseSolver, ISolver
    {
        private bool _movePerformed;
        public Stopwatch Stopwatch { get; set; } = new Stopwatch();

        public IEnumerable<IMoveFactory> MoveFactories { get; set; }

        private List<TvBreak> _breakInOrder { get; set; }
        public string Description { get; set; }

        public bool StopWhenCompleted { get; set; } = true;
        public Action ActionWhenScoreDecrease { get; set; } = Action.Ignore;
        public TimeSpan MaxTime { get; set; }
        public bool PropagateRandomSeed { get; set; }

        public List<ISolver> InitialSolvers { get; set; } = new List<ISolver>();

        public LocalSearchSolver() : base()
        {
        }

        public void Solve()
        {
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
            foreach (var solver in InitialSolvers)
            {
                solver.Instance = Instance;
                if (PropagateRandomSeed)
                {
                    solver.Seed = Random.Next();
                }
                solver.ScoringFunction = ScoringFunction;
                solver.Solve();
                Solution = solver.Solution;
            }

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
                        PositionsCountLimit = 4,
                        MaxTasksChecked = 3,
                        MaxBreaksChecked = 3,
                        IgnoreBreaksWhenUnitOverfillAbove = 60,
                        IgnoreCompletedTasks = true,
                        IgnoreTasksWithCompletedViews = false,
                    },
                    new InsertMoveFactory()
                    {
                        MildlyRandomOrder = true,
                        PositionsCountLimit = 4,
                        MaxTasksChecked = 3,
                        MaxBreaksChecked = 3,
                        IgnoreBreaksWhenUnitOverfillAbove = 60,
                        IgnoreCompletedTasks = false,
                        IgnoreTasksWithCompletedViews = false,
                    },
                    new DeleteMoveFactory()
                    {
                        MildlyRandomOrder = true,
                        PositionsCountLimit = 3,
                        MaxBreaksChecked = 3,
                    },
                    new SwapMoveFactory()
                    {
                        MildlyRandomOrder = true,
                        PositionsCountLimit = 3,
                        MaxTasksChecked = 3,
                        MaxBreaksChecked = 3,
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
            if (candidate.OverallDifference.HasScoreWorsened())
            {
                if(ActionWhenScoreDecrease == Action.Stop)
                {
                    return;
                }
                else if(ActionWhenScoreDecrease == Action.Ignore)
                {
                    _movePerformed = true;
                    return;
                }
            }
            candidate.Execute();
            Solution.GradingFunction.RecalculateSolutionScoresBasedOnTaskData(Solution);
            _movePerformed = true;
        }
    }
}
