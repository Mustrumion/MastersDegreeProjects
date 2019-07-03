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
        
        public int PositionsPerBreakTakenIntoConsideration { get; set; } = 0;
        public int MaxBreakExtensionUnits { get; set; } = 20;

        private List<TvBreak> _breakInOrder { get; set; }
        public string Description { get; set; }

        public InsertionHeuristic() : base()
        {
        }

        public void Solve()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (!Solution.Scored)
            {
                ScoringFunction.AssesSolution(Solution);
            }
            Solution.Scored = false;
            //due earlier are scheduled first
            //heftier are scheduled first if due at the same time
            var ordersInOrder = Instance.AdOrders.Values.OrderByDescending(order => order.AdSpanUnits).OrderBy(order => order.DueTime).ToList();
            _breakInOrder = Instance.Breaks.Values.OrderBy(b => b.StartTime).ToList();

            _movePerformed = true;
            while (Solution.CompletionScore < 1 && _movePerformed)
            {
                _movePerformed = false;
                foreach (AdvertisementTask order in ordersInOrder.Where(o => Solution.AdOrderData[o.ID].Completed))
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
                if(!move.OverallDifference.HasScoreImproved() && !move.OverallDifference.AnyCompatibilityIssuesIncreased())
                {
                    move.Execute();
                    _movePerformed = true;
                    return;
                }
            }
            var candidate = moves.OrderBy(m => m.OverallDifference.IntegrityLossScore).FirstOrDefault();
            if(candidate.OverallDifference.IntegrityLossScore < 0)
            {
                candidate.Execute();
                _movePerformed = true;
            }
        }


        private void TryToScheduleOrder(AdvertisementTask order)
        {
            foreach(var tvBreak in _breakInOrder)
            {
                var schedule = Solution.AdvertisementsScheduledOnBreaks[tvBreak.ID];
                if(schedule.UnitFill - MaxBreakExtensionUnits > tvBreak.SpanUnits)
                {
                    continue;
                }
                InsertMoveFactory factory = new InsertMoveFactory(Solution)
                {
                    Breaks = new[] { tvBreak },
                    Tasks = new[] { order },
                    MildlyRandomOrder = false,
                    PositionsCountLimit = PositionsPerBreakTakenIntoConsideration,
                    Random = Random,
                };
                List<IMove> moves = factory.GenerateMoves().ToList();
                ChooseMoveToPerform(moves);
            }
        }
    }
}
