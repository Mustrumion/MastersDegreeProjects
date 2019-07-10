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
    public class BeginingsHeuristic : BaseSingleGoalHeuristic, ISolver
    {
        public int MaxBreakExtensionUnits { get; set; } = 20;

        public BeginingsHeuristic() : base()
        {
        }


        private void PerformIfTransformationImprovesScore(TaskScore taskScore, BreakSchedule breakSchedule)
        {
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
            int nextPos = breakPositions.Count > 0 ? breakPositions[0] : 999999999;
            if (taskScore.AdConstraints.MinJobsBetweenSame > nextPos) return false;
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
                if (orderData.StartsSatisfied || CurrentTime.Elapsed >= TimeLimit)
                {
                    break;
                }
            }
        }

        protected override void PerformLoop()
        {
            var orders = Solution.AdOrdersScores.Values.Where(o => !o.StartsSatisfied).ToList();
            orders.Shuffle(Random);
            foreach (TaskScore order in orders)
            {
                TryToScheduleOrder(order);
                if (CurrentTime.Elapsed >= TimeLimit) break;
            }
        }
    }
}
