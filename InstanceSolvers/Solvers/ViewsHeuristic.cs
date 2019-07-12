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
    public class ViewsHeuristic : BaseGreedyTransformationHeuristic, ISolver
    {
        public int MaxInsertedPerBreak { get; set; } = 99999;
        public int MaxBreakExtensionUnits { get; set; } = 20;

        public ViewsHeuristic() : base()
        {
        }
        
        
        private void ChooseMoveToPerform(List<int> positions, TaskScore taskScore, BreakSchedule breakSchedule)
        {
            foreach(var position in positions)
            {
                Insert move = new Insert()
                {
                    Solution = Solution,
                    Position = position,
                    TvBreak = breakSchedule.BreakData,
                    AdvertisementOrder = taskScore.AdConstraints,
                };
                move.Asses();
                if(move.OverallDifference.HasScoreImproved() && !move.OverallDifference.AnyCompatibilityIssuesIncreased())
                {
                    move.Execute();
                    _numberOfMoves += 1;
                    _movePerformed = true;
                }
                else
                {
                    break;
                }
                if (CurrentTime.Elapsed >= TimeLimit) break;
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
                if (added.Count >= MaxInsertedPerBreak) break;
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
                arrIndex += 1;
            }
            return added;
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
                var possibilities = GetPossibleInserts(orderData, schedule);
                ChooseMoveToPerform(possibilities, orderData, schedule);
                if((orderData.ViewsSatisfied && orderData.TimesAiredSatisfied) || CurrentTime.Elapsed >= TimeLimit)
                {
                    break;
                }
            }
        }

        protected override void PerformLoop()
        {
            var orders = Solution.AdOrdersScores.Values.Where(o => !o.TimesAiredSatisfied || !o.ViewsSatisfied).ToList();
            orders.Shuffle(Random);
            foreach (TaskScore order in orders)
            {
                TryToScheduleOrder(order);
                if (CurrentTime.Elapsed >= TimeLimit) break;
            }
        }
    }
}
