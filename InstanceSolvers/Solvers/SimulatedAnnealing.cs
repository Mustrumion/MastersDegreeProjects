using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using InstanceSolvers.TransformationFactories;
using InstanceSolvers.Transformations;
using InstanceSolvers.Solvers.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers.Solvers
{
    public class SimulatedAnnealing : BaseSolver, ISolver
    {
        private Solution _previousBest;
        private ITransformationFactory _bestFactory;
        private ITransformation _bestMove;
        private int _numberOfLoopsWithoutImprovement;
        private bool _timeToStop;

        public IEnumerable<ITransformationFactory> MoveFactories { get; set; }
        public bool StopWhenCompleted { get; set; }
        public int NumberOfNoGoodActionsToStop { get; set; }
        public double NeighberhoodAdjustmentParam { get; set; }
        public double BestFactoryAdjustmentParam { get; set; }
        [JsonIgnore]
        public int NumberOfMoves { get; set; }
        public double ImprovementOverNarrowNeighb { get; set; }

        public SimulatedAnnealing() : base()
        {
        }

        private void ReinitializePrivates()
        {
            _previousBest = null;
            _bestFactory = null;
            _bestMove = null;
            _numberOfLoopsWithoutImprovement = 0;
            _timeToStop = false;
        }

        protected override void InternalSolve()
        {
            ReinitializePrivates();
            InitializeMoveFactories();
            while (!TimeToEnd())
            {
                _bestFactory = null;
                _bestMove = null;
                foreach (ITransformationFactory factory in MoveFactories)
                {
                    if (TimeToEnd()) break;
                    AssesMovesFromFactory(factory);
                }
                ChooseToPerform();
            }
            if (_previousBest != null && _previousBest.IsBetterThan(Solution))
            {
                Solution = _previousBest;
                _previousBest.RestoreStructures();
                _scoringFunction.AssesSolution(Solution);
            }
            if (DiagnosticMessages) Console.WriteLine($"Number of transformations performed {NumberOfMoves}.");
        }

        private bool FirstIsBetter(ITransformation move1, ITransformation move2)
        {
            if (move2 == null && move1 != null) return true;
            if (move1.OverallDifference.IntegrityLossScore < move2.OverallDifference.IntegrityLossScore) return true;
            if (move1.OverallDifference.IntegrityLossScore == move2.OverallDifference.IntegrityLossScore && move1.OverallDifference.WeightedLoss < move2.OverallDifference.WeightedLoss) return true;
            return false;
        }

        private void AssesMovesFromFactory(ITransformationFactory factory)
        {
            var moves = factory.GenerateMoves().ToList();
            foreach (var move in moves)
            {
                if (TimeToEnd()) break;
                move.Asses();
                if (FirstIsBetter(move, _bestMove))
                {
                    _bestMove = move;
                    _bestFactory = factory;
                }
            }
        }

        private void InitializeMoveFactories()
        {
            if (MoveFactories == null)
            {
                MoveFactories = new List<ITransformationFactory>
                {
                    new InsertFactory()
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
                    new InsertFactory()
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
                    new DeleteFactory()
                    {
                        MildlyRandomOrder = true,
                        PositionsCountLimit = 3,
                        MaxBreaksChecked = 3,
                        AlwaysReturnStartsAndEnds = true,
                    },
                    new SwapFactory()
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
                if (PropagateRandomSeed)
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

        private bool TimeToEnd(bool outer = true)
        {
            if(Solution.CompletionScore >= 1)
            {
                if(StopWhenCompleted || Solution.WeightedLoss == 0)
                {
                    if (DiagnosticMessages  && outer) Console.WriteLine($"TaskCompleted.");
                    return true;
                }
            }
            if (CurrentTime.Elapsed > TimeLimit)
            {
                if (DiagnosticMessages && outer) Console.WriteLine($"Timeout of {TimeLimit}.");
                return true;
            }
            if (NumberOfNoGoodActionsToStop != 0 && NumberOfNoGoodActionsToStop <= _numberOfLoopsWithoutImprovement)
            {
                if (DiagnosticMessages && outer) Console.WriteLine($"Performed {_numberOfLoopsWithoutImprovement} actions with no improvement.");
                return true;
            }
            if (_timeToStop)
            {
                if (DiagnosticMessages && outer) Console.WriteLine($"No good action.");
                return true;
            }
            return false;
        }


        private void WidenNeighberhood()
        {
            if (NeighberhoodAdjustmentParam == 0) return;
            foreach (var factory in MoveFactories)
            {
                factory.WidenNeighborhood(NeighberhoodAdjustmentParam * ImprovementOverNarrowNeighb);
            }
        }

        private void NarrowNeighberhood()
        {
            if (NeighberhoodAdjustmentParam == 0) return;
            foreach (var factory in MoveFactories)
            {
                factory.NarrowNeighborhood(NeighberhoodAdjustmentParam);
            }
        }

        private void RewardBestFactory()
        {
            if (BestFactoryAdjustmentParam == 0 || _bestFactory == null) return;
            _bestFactory.WidenNeighborhood(BestFactoryAdjustmentParam);
            //Console.WriteLine($"Rewarded {_bestFactory.GetType().Name}");
        }

        private void ChooseToPerform()
        {
            if (_bestMove == null || !_bestMove.OverallDifference.HasScoreImproved())
            {
                WidenNeighberhood();
                _numberOfLoopsWithoutImprovement += 1;
            }
            else
            {
                _numberOfLoopsWithoutImprovement = 0;
                RewardBestFactory();
                NarrowNeighberhood();
                NumberOfMoves += 1;
            }
            if(_bestMove == null || _bestMove.OverallDifference.HasScoreWorsened())
            {
                if (Solution.IsBetterThan(_previousBest))
                {
                    _previousBest = Solution.TakeSnapshot();
                }
            }
            if (_bestMove == null) return;
            _bestMove.Execute();
            Reporter.AddEntry(_bestMove.GenerateReportEntry());
        }
    }
}
