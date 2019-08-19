using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using InstanceSolvers.TransformationFactories;
using InstanceSolvers.Transformations;
using InstanceSolvers.Solvers.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers.Solvers
{
    public class ImprovingInsertsHeuristic : BaseSolver, ISolver
    {
        private bool _movePerformed;
        
        public int PositionsPerBreakTakenIntoConsideration { get; set; } = 0;
        public int MaxBreakExtensionUnits { get; set; } = 20;

        private List<TvBreak> _breakInOrder { get; set; }

        public ImprovingInsertsHeuristic() : base()
        {
        }
        

        private void ChooseMoveToPerform(List<ITransformation> moves)
        {
            foreach(var move in moves)
            {
                move.Asses();
            }
            var candidate = moves.OrderBy(m => m.OverallDifference.IntegrityLossScore).FirstOrDefault();
            if(candidate.OverallDifference.IntegrityLossScore < 0)
            {
                candidate.Execute();
                Reporter.AddEntry(candidate.GenerateReportEntry());
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
                InsertFactory factory = new InsertFactory(Solution)
                {
                    Breaks = new[] { tvBreak },
                    Tasks = new[] { order },
                    MildlyRandomOrder = false,
                    PositionsCountLimit = PositionsPerBreakTakenIntoConsideration,
                    Random = Random,
                };
                List<ITransformation> moves = factory.GenerateMoves().ToList();
                ChooseMoveToPerform(moves);
                if(TimeLimit < CurrentTime.Elapsed)
                {
                    break;
                }
            }
        }

        protected override void InternalSolve()
        {
            //due earlier are scheduled first
            //heftier are scheduled first if due at the same time
            var ordersInOrder = Instance.AdOrders.Values.OrderByDescending(order => order.AdSpanUnits).OrderBy(order => order.DueTime).ToList();
            _breakInOrder = Instance.Breaks.Values.OrderBy(b => b.StartTime).ToList();

            _movePerformed = true;
            while (Solution.CompletionScore < 1 && _movePerformed && TimeLimit >= CurrentTime.Elapsed)
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
