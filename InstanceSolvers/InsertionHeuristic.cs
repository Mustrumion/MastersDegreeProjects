using InstanceGenerator;
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
    public class InsertionHeuristic : BaseSolver, ISolver
    {
        private bool _movePerformed;

        public List<ISolver> InitialSolvers { get; set; } = new List<ISolver>();

        public int MaxInsertedPerBreak { get; set; } = 0;
        public int MaxBreakExtensionUnits { get; set; } = 20;
        public bool PropagateRandomnessSeed { get; set; } = true;
        public int MovesPerformed { get; set; }
        
        public string Description { get; set; }

        public InsertionHeuristic() : base()
        {
        }

        private void FireInitialSolvers()
        {
            foreach (var solver in InitialSolvers)
            {
                solver.Instance = Instance;
                if (PropagateRandomnessSeed)
                {
                    solver.Seed = Random.Next();
                }
                solver.ScoringFunction = ScoringFunction;
                solver.Solve();
                Solution = solver.Solution;
            }
        }

        public void Solve()
        {
            FireInitialSolvers();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (!Solution.Scored)
            {
                ScoringFunction.AssesSolution(Solution);
            }
            Solution.Scored = false;

            _movePerformed = true;
            while (Solution.CompletionScore < 1 && _movePerformed)
            {
                _movePerformed = false;
                var orders = Solution.AdOrdersScores.Values.Where(o => !o.TimesAiredSatisfied || !o.ViewsSatisfied).ToList();
                orders.Shuffle(Random);
                foreach (TaskScore order in orders)
                {
                    TryToScheduleOrder(order);
                }
            }
            Solution.GradingFunction.RecalculateSolutionScoresBasedOnTaskData(Solution);
            Solution.Scored = true;
            stopwatch.Stop();
            Solution.TimeElapsed += stopwatch.Elapsed;
        }


        private void ChooseMoveToPerform(List<IMove> moves)
        {
            foreach(var move in moves)
            {
                move.Asses();
                if(move.OverallDifference.HasScoreImproved() && !move.OverallDifference.AnyCompatibilityIssuesIncreased())
                {
                    move.Execute();
                    MovesPerformed += 1;
                    _movePerformed = true;
                    return;
                }
            }
        }


        private List<int> GetPossibleInserts(TaskScore taskScore, BreakSchedule breakSchedule)
        {
            List<int> added = new List<int>();
            if(!taskScore.BreaksPositions.TryGetValue(breakSchedule.ID, out var breakPositions))
            {
                breakPositions = new List<int>();
            }
            breakPositions.Sort();
            breakPositions = breakPositions.ToList();
            int arrIndex = 0;
            for(int possiblePos = 0; possiblePos < breakSchedule.Order.Count + 1; )
            {
                if (breakPositions.Count >= taskScore.AdConstraints.MaxPerBlock) break;
                if (breakSchedule.UnitFill + (added.Count + 1) * taskScore.AdConstraints.AdSpanUnits > breakSchedule.BreakData.SpanUnits + MaxBreakExtensionUnits) break;
                int nextPos = breakPositions.Count > arrIndex ? breakPositions[arrIndex] : 999999999;
                if (possiblePos + taskScore.AdConstraints.MinJobsBetweenSame <= nextPos)
                {
                    added.Add(possiblePos);
                    for(int j = arrIndex; j < breakPositions.Count; j++)
                    {
                        breakPositions[j] += 1;
                    }
                    breakPositions.Insert(arrIndex, possiblePos);
                    possiblePos += taskScore.AdConstraints.MinJobsBetweenSame + 1;
                }
                else
                {
                    possiblePos = nextPos + taskScore.AdConstraints.MinJobsBetweenSame + 1;
                }
            }
            return added;
        }


        private void TryToScheduleOrder(TaskScore orderData)
        {
            var schedules = Solution.AdvertisementsScheduledOnBreaks.Values.Where(s =>
                {
                    if (s.UnitFill > MaxBreakExtensionUnits + s.BreakData.SpanUnits) return false;
                    if (Solution.GetTypeToBreakIncompatibility(orderData, s) == 1) return false;
                    if (Solution.GetBulkBrandIncompatibilities(orderData.AdConstraints, s.Order).Contains(double.PositiveInfinity)) return false;
                    return true;
                }).ToList();
            schedules.Shuffle(Random);
            foreach(var schedule in schedules)
            {
                var possibilities = GetPossibleInserts(orderData, schedule);

                InsertMoveFactory factory = new InsertMoveFactory(Solution)
                {
                    Breaks = new[] { schedule.BreakData },
                    Tasks = new[] { orderData.AdConstraints },
                    MildlyRandomOrder = true,
                    PositionsCountLimit = MaxInsertedPerBreak,
                    Random = Random,
                };
                List<IMove> moves = factory.GenerateMoves().ToList();
                ChooseMoveToPerform(moves);
            }
        }
    }
}
