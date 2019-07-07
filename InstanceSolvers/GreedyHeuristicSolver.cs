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
    public class GreedyHeuristicSolver : BaseSolver, ISolver
    {
        private bool _movePerformed;
        
        public int PositionsPerBreakTakenIntoConsideration { get; set; } = 0;
        public int MaxBreakExtensionUnits { get; set; } = 20;

        private List<TvBreak> _breakInOrder { get; set; }

        public GreedyHeuristicSolver() : base()
        {
        }
        

        private void ChooseMoveToPerform(List<IMove> moves)
        {
            foreach(var move in moves)
            {
                move.Asses();
            }
            var candidate = moves.OrderBy(m => m.OverallDifference.IntegrityLossScore).FirstOrDefault();
            if(candidate.OverallDifference.IntegrityLossScore < 0)
            {
                candidate.Execute();
                Solution.GradingFunction.RecalculateSolutionScoresBasedOnTaskData(Solution);
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

        protected override void InternalSolve()
        {
            //due earlier are scheduled first
            //heftier are scheduled first if due at the same time
            var ordersInOrder = Instance.AdOrders.Values.OrderByDescending(order => order.AdSpanUnits).OrderBy(order => order.DueTime).ToList();
            _breakInOrder = Instance.Breaks.Values.OrderBy(b => b.StartTime).ToList();

            _movePerformed = true;
            while (Solution.CompletionScore < 1 && _movePerformed)
            {
                _movePerformed = false;
                foreach (AdvertisementTask order in ordersInOrder)
                {
                    TryToScheduleOrder(order);
                }
            }
        }
    }
}
