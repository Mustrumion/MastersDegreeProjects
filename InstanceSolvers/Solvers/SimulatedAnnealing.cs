﻿using InstanceGenerator.InstanceData;
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
        private int _stepsSinceLastImprovement;
        private int _loopsSinceLastAction;
        private bool _timeToStop;

        public IEnumerable<ITransformationFactory> MoveFactories { get; set; }
        public bool StopWhenCompleted { get; set; } = false;
        public int StepsAnalyzedWithoutImprovementToStop { get; set; } = 0;
        public int NumberOfLoopsWithoutActionsToStop { get; set; } = 1;

        public bool AllowIntegrityLoss { get; set; } = false;
        public double IntegrityLossMultiplier { get; set; } = 1000.0;

        public double FunctionDeltaOffset { get; set; } = 20000.0;
        public double StartingTemperature { get; set; } = 1000000.0;
        public double TemperatureMultiplier { get; set; } = 1.0;
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
            _stepsSinceLastImprovement = 0;
            _loopsSinceLastAction = 0;
            _timeToStop = false;
        }

        protected override void InternalSolve()
        {
            ReinitializePrivates();
            InitializeMoveFactories();
            while (!TimeToEnd())
            {
                _loopsSinceLastAction += 1;
                List<IEnumerator<ITransformation>> moveQueues = new List<IEnumerator<ITransformation>>();
                foreach (ITransformationFactory factory in MoveFactories)
                {
                    moveQueues.Add(factory.GenerateMoves().GetEnumerator());
                }
                while (!TimeToEnd())
                {
                    if(moveQueues.Count == 0)
                    {
                        break;
                    }
                    IEnumerator<ITransformation> moveQueue = moveQueues[Random.Next() % moveQueues.Count];
                    if (!moveQueue.MoveNext())
                    {
                        moveQueues.Remove(moveQueue);
                        continue;
                    }
                    ITransformation move = moveQueue.Current;
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

        private bool FirstIsBetter(ITransformation move1, ITransformation move2)
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
                MoveFactories = new List<ITransformationFactory>
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
                    new InsertFactory()
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
            if (StepsAnalyzedWithoutImprovementToStop != 0 && StepsAnalyzedWithoutImprovementToStop < _stepsSinceLastImprovement)
            {
                if (DiagnosticMessages && outer) Console.WriteLine($"Analyzed {_stepsSinceLastImprovement} steps with no improvement.");
                return true;
            }
            if (NumberOfLoopsWithoutActionsToStop != 0 && NumberOfLoopsWithoutActionsToStop < _loopsSinceLastAction)
            {
                if (DiagnosticMessages && outer) Console.WriteLine($"Performed {_loopsSinceLastAction} loops with no solution tranformation.");
                return true;
            }
            if (_timeToStop)
            {
                if (DiagnosticMessages && outer) Console.WriteLine($"No good action.");
                return true;
            }
            return false;
        }

        private bool ChooseToPerform(ITransformation move)
        {
            NumberOfSteps += 1;
            move.Asses();
            if (move.OverallDifference.HasScoreImproved())
            {
                _stepsSinceLastImprovement = 0;
            }
            else
            {
                _stepsSinceLastImprovement += 1;
                double transformationDelta;
                if (move.OverallDifference.IntegrityLossScore > 0.0)
                {
                    if (!AllowIntegrityLoss) return false;
                    transformationDelta = move.OverallDifference.IntegrityLossScore * IntegrityLossMultiplier;
                }
                else
                {
                    transformationDelta = move.OverallDifference.WeightedLoss;
                }
                transformationDelta += FunctionDeltaOffset;
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
            _loopsSinceLastAction = 0;
            NumberOfMoves += 1;
            move.Execute();
            Reporter.AddEntry(move.GenerateReportEntry());
            return true;
        }
    }
}
