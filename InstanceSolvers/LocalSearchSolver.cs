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

        public IEnumerable<IMoveFactory> MoveFactories { get; set; }

        private List<TvBreak> _breakInOrder { get; set; }
        public bool StopWhenCompleted { get; set; } = true;
        public Action ActionWhenScoreDecrease { get; set; } = Action.Ignore;
        public bool PropagateRandomnessSeed { get; set; }
        public int NumberOfMoves { get; set; }
        public List<IMove> MovesPerformed { get; set; } = new List<IMove>();

        public LocalSearchSolver() : base()
        {
        }
        
        protected override void InternalSolve()
        {
            InitializeMoveFactories();
            _movePerformed = true;
            while (_movePerformed)
            {
                _movePerformed = false;
                List<IMove> moves = new List<IMove>();
                foreach (IMoveFactory factory in MoveFactories)
                {
                    moves.AddRange(factory.GenerateMoves().ToList());
                }
                ChooseMoveToPerform(moves);
            }
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
                        AlwaysReturnStartsAndEnds = true,
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
                        AlwaysReturnStartsAndEnds = true,
                    },
                    new DeleteMoveFactory()
                    {
                        MildlyRandomOrder = true,
                        PositionsCountLimit = 3,
                        MaxBreaksChecked = 3,
                        AlwaysReturnStartsAndEnds = true,
                    },
                    new SwapMoveFactory()
                    {
                        MildlyRandomOrder = true,
                        PositionsCountLimit = 3,
                        MaxTasksChecked = 3,
                        MaxBreaksChecked = 3,
                        AlwaysReturnStartsAndEnds = true,
                    },
                };
            }
            foreach (var moveFactory in MoveFactories)
            {
                if (PropagateRandomnessSeed)
                {
                    moveFactory.Seed = Random.Next();
                }
                else
                {
                    moveFactory.Seed = (Random.Next() + new Random().Next()) % int.MaxValue;
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
            if(CurrentTime.Elapsed > TimeLimit)
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
            MovesPerformed.Add(candidate);
            candidate.CleanData();
            _movePerformed = true;
        }
    }
}
