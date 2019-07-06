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

        public int PositionsPerBreakTakenIntoConsideration { get; set; } = 0;
        public int MaxBreakExtensionUnits { get; set; } = 20;
        public bool PropagateRandomnessSeed { get; set; } = true;
        
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
                foreach (TaskData order in orders)
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


        private void TryToScheduleOrder(TaskData orderData)
        {
            var schedules = Solution.AdvertisementsScheduledOnBreaks.Values.Where(s =>
                {
                    if (s.UnitFill > MaxBreakExtensionUnits + s.BreakData.SpanUnits) return false;
                    if (Solution.GetTypeToBreakIncompatibility(orderData, s) == 1) return false;
                    if (Solution.GetBulkBrandIncompatibilities(orderData.AdvertisementOrderData, s.Order).Contains(double.PositiveInfinity)) return false;
                    return true;
                }).ToList();
            schedules.Shuffle(Random);
            foreach(var schedule in schedules)
            {
                InsertMoveFactory factory = new InsertMoveFactory(Solution)
                {
                    Breaks = new[] { schedule.BreakData },
                    Tasks = new[] { orderData.AdvertisementOrderData },
                    MildlyRandomOrder = true,
                    PositionsCountLimit = PositionsPerBreakTakenIntoConsideration,
                    Random = Random,
                };
                List<IMove> moves = factory.GenerateMoves().ToList();
                ChooseMoveToPerform(moves);
            }
        }
    }
}
