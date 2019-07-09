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
    public class EndingsHeuristic : BaseSolver, ISolver
    {
        private bool _movePerformed;
        
        public int MaxLoops { get; set; } = 9999999;
        public int MaxBreakExtensionUnits { get; set; } = 20;
        public int NumberOfMoves { get; set; }
        public int LoopsPerformed { get; set; }

        public bool DiagnosticMessages { get; set; } = false;

        public EndingsHeuristic() : base()
        {
        }
        


        private void PerformIfTransformationImprovesScore(TaskScore taskScore, BreakSchedule breakSchedule)
        {
            if(CurrentTime.Elapsed > TimeLimit)
            {
                return;
            }
            Insert move = new Insert()
            {
                Solution = Solution,
                Position = 0,
                TvBreak = breakSchedule.BreakData,
                AdvertisementOrder = taskScore.AdConstraints,
            };
            move.Asses();
            if(move.OverallDifference.HasScoreImproved() && !move.OverallDifference.AnyCompatibilityIssuesIncreased())
            {
                move.Execute();
                NumberOfMoves += 1;
                _movePerformed = true;
            }
        }



        private bool CheckForNoSelfConflicts(TaskScore taskScore, BreakSchedule breakSchedule)
        {
            if (!taskScore.BreaksPositions.TryGetValue(breakSchedule.ID, out var breakPositions))
            {
                breakPositions = new List<int>();
            }
            breakPositions.Sort();
            if (breakPositions.Count >= taskScore.AdConstraints.MaxPerBlock) return false;
            if (breakSchedule.UnitFill + taskScore.AdConstraints.AdSpanUnits > breakSchedule.BreakData.SpanUnits + MaxBreakExtensionUnits) return false;
            int lastPos = breakPositions.Count > 0 ? breakPositions.Last() : 999999999;
            if (Math.Abs(lastPos - breakSchedule.Count - 1) < taskScore.AdConstraints.MinJobsBetweenSame) return false;
            return true;
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
                if(CheckForNoSelfConflicts(orderData, schedule))
                {
                    PerformIfTransformationImprovesScore(orderData, schedule);
                }
                //var possibilities = GetPossibleInserts(orderData, schedule);
                //ChooseMoveToPerform(possibilities, orderData, schedule);
                if (orderData.EndsSatisfied)
                {
                    break;
                }
            }
        }


        private bool TimeToEnd()
        {
            if (Solution.CompletionScore >= 1) return true;
            if (!_movePerformed) return true;
            if (LoopsPerformed >= MaxLoops) return true;
            if (CurrentTime.Elapsed >= TimeLimit) return true;
            return false;
        }

        protected override void InternalSolve()
        {
            _movePerformed = true;
            while (!TimeToEnd())
            {
                _movePerformed = false;
                var orders = Solution.AdOrdersScores.Values.Where(o => !o.EndsSatisfied).ToList();
                orders.Shuffle(Random);
                foreach (TaskScore order in orders)
                {
                    TryToScheduleOrder(order);
                }
                LoopsPerformed += 1;
            }
            Solution.GradingFunction.RecalculateSolutionScoresBasedOnTaskData(Solution);
            Solution.Scored = true;
            if(DiagnosticMessages) Console.WriteLine($"Endings heuristic ended. Number of moves: {NumberOfMoves}. LoopsPerformed: {LoopsPerformed}.");
        }
    }
}
