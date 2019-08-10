using InstanceGenerator;
using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using InstanceSolvers.MoveFactories;
using InstanceSolvers.Moves;
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
        private int _loopsSinceLastImprovemnt;
        private bool _timeToStop;

        public IEnumerable<IMoveFactory> MoveFactories { get; set; }
        public bool StopWhenCompleted { get; set; } = false;
        public int NumberOfLoopsWithoutImprovementToStop { get; set; } = 20;
        public double StartingTemperature { get; set; } = 1.0;
        public double TemperatureMultiplier { get; set; } = 1.0;
        public double IntegrityLossMultiplier { get; set; } = 1000.0;
        [JsonIgnore]
        public int NumberOfMoves { get; set; }
        [JsonIgnore]
        public int NumberOfSteps { get; set; }

        public SimulatedAnnealing() : base()
        {
        }

        private void ReinitializePrivates()
        {
            _previousBest = null;
            _loopsSinceLastImprovemnt = 0;
            _timeToStop = false;
        }

        protected override void InternalSolve()
        {
            ReinitializePrivates();
            InitializeMoveFactories();
            while (!TimeToEnd())
            {
                _loopsSinceLastImprovemnt += 1;
                List<IEnumerator<IMove>> moveQueues = new List<IEnumerator<IMove>>();
                foreach (IMoveFactory factory in MoveFactories)
                {
                    moveQueues.Add(factory.GenerateMoves().GetEnumerator());
                }
                while (!TimeToEnd())
                {
                    if(moveQueues.Count == 0)
                    {
                        break;
                    }
                    IEnumerator<IMove> moveQueue = moveQueues[Random.Next() % moveQueues.Count];
                    if (!moveQueue.MoveNext())
                    {
                        moveQueues.Remove(moveQueue);
                        continue;
                    }
                    IMove move = moveQueue.Current;
                    moveQueue.MoveNext();
                    if (ChooseToPerform(move))
                    {
                        break;
                    }
                }
            }
            if (_previousBest != null && _previousBest.IsBetterThan(Solution))
            {
                Solution = _previousBest;
                _previousBest.RestoreStructures();
                _scoringFunction.AssesSolution(Solution);
            }
            if (DiagnosticMessages) Console.WriteLine($"Number of transformations performed {NumberOfMoves}.");
        }

        private bool FirstIsBetter(IMove move1, IMove move2)
        {
            if (move2 == null && move1 != null) return true;
            if (move1.OverallDifference.IntegrityLossScore < move2.OverallDifference.IntegrityLossScore) return true;
            if (move1.OverallDifference.IntegrityLossScore == move2.OverallDifference.IntegrityLossScore && move1.OverallDifference.WeightedLoss < move2.OverallDifference.WeightedLoss) return true;
            return false;
        }

        private void InitializeMoveFactories()
        {
            if (MoveFactories == null)
            {
                MoveFactories = new List<IMoveFactory>
                {
                    new RandomInsertFactory()
                    {
                        MildlyRandomOrder = true,
                        MovesReturned = 99999999,
                    },
                    new RandomDeleteFactory()
                    {
                        MildlyRandomOrder = true,
                        MovesReturned = 99999999,
                    },
                    new RandomSwapFactory()
                    {
                        MildlyRandomOrder = true,
                        MovesReturned = 99999999,
                    },
                    new InsertMoveFactory()
                    {
                        MildlyRandomOrder = true,
                        PositionsCountLimit = 4,
                        MaxTasksChecked = 3,
                        MaxBreaksChecked = 3,
                        IgnoreCompletedTasks = false,
                        IgnoreTasksWithCompletedViews = false,
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
            if (NumberOfLoopsWithoutImprovementToStop != 0 && NumberOfLoopsWithoutImprovementToStop < _loopsSinceLastImprovemnt)
            {
                if (DiagnosticMessages && outer) Console.WriteLine($"Performed {_loopsSinceLastImprovemnt} actions with no improvement.");
                return true;
            }
            if (_timeToStop)
            {
                if (DiagnosticMessages && outer) Console.WriteLine($"No good action.");
                return true;
            }
            return false;
        }

        private bool ChooseToPerform(IMove move)
        {
            NumberOfSteps += 1;
            move.Asses();
            if (move.OverallDifference.HasScoreImproved())
            {
                _loopsSinceLastImprovemnt = 0;
            }
            else
            {
                double transformationDelta;
                if (move.OverallDifference.IntegrityLossScore > 0.0)
                {
                    transformationDelta = move.OverallDifference.IntegrityLossScore * IntegrityLossMultiplier;
                }
                else
                {
                    transformationDelta = move.OverallDifference.WeightedLoss;
                }
                transformationDelta += 1;
                double currentTemperature = StartingTemperature / Math.Log(NumberOfSteps);
                double p = Math.Exp(-transformationDelta/currentTemperature);
                Console.WriteLine(p);
                if(Random.NextDouble() > p) return false;
            }
            if(move.OverallDifference.HasScoreWorsened())
            {
                if (Solution.IsBetterThan(_previousBest))
                {
                    _previousBest = Solution.TakeSnapshot();
                }
            }
            NumberOfMoves += 1;
            move.Execute();
            Reporter.AddEntry(move.GenerateReportEntry());
            return true;
        }
    }
}
