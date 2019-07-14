using InstanceGenerator;
using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using InstanceSolvers.MoveFactories;
using InstanceSolvers.Moves;
using InstanceSolvers.Solvers.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers.Solvers
{
    public class EndingsHeuristic : BaseGreedyTransformationHeuristic, ISolver
    {
        
        public int MaxBreakExtensionUnits { get; set; } = 20;

        public EndingsHeuristic() : base()
        {
        }
        
        
        private void PerformIfTransformationImprovesScore(TaskScore taskScore, BreakSchedule breakSchedule)
        {
            Insert move = new Insert()
            {
                Solution = Solution,
                Position = breakSchedule.Count,
                TvBreak = breakSchedule.BreakData,
                AdvertisementOrder = taskScore.AdConstraints,
            };
            move.Asses();
            if(move.OverallDifference.HasScoreImproved() && !move.OverallDifference.AnyCompatibilityIssuesIncreased())
            {
                move.Execute();
                Reporter.AddEntry(move.GenerateReportEntry());
                _numberOfMoves += 1;
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
            if (Math.Abs(lastPos - breakSchedule.Count) < taskScore.AdConstraints.MinJobsBetweenSame) return false;
            return true;
        }

        private void TryToScheduleOrder(TaskScore orderData)
        {
            var schedules = Solution.AdvertisementsScheduledOnBreaks.Values.Where(s =>
                {
                    if (s.UnitFill > MaxBreakExtensionUnits + s.BreakData.SpanUnits) return false;
                    if (Instance.GetTypeToBreakIncompatibility(orderData, s) == 1) return false;
                    if (Instance.GetBulkBrandIncompatibilities(orderData.AdConstraints, s.Order).Contains(double.PositiveInfinity)) return false;
                    return true;
                }).ToList();
            schedules.Shuffle(Random);
            foreach(var schedule in schedules)
            {
                if(CheckForNoSelfConflicts(orderData, schedule))
                {
                    PerformIfTransformationImprovesScore(orderData, schedule);
                }
                if (orderData.EndsSatisfied || CurrentTime.Elapsed >= TimeLimit)
                {
                    break;
                }
            }
        }
        

        protected override void PerformLoop()
        {
            var orders = Solution.AdOrdersScores.Values.Where(o => !o.EndsSatisfied).ToList();
            orders.Shuffle(Random);
            foreach (TaskScore order in orders)
            {
                TryToScheduleOrder(order);
                if (CurrentTime.Elapsed >= TimeLimit) break;
            }
        }
    }
}
