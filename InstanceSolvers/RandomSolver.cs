using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using InstanceSolvers.Moves;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers
{
    public class RandomSolver : BaseSolver, ISolver
    {
        public string Description { get; set; }

        public RandomSolver() : base()
        {
        }
        
        
        private bool InsertInRandomNonFilledBreak(TaskScore taskData)
        {
            List<BreakSchedule> breaksWithEnoughSpace = Solution.AdvertisementsScheduledOnBreaks.Values.Where
                (
                    b => b.BreakData.SpanUnits >= b.UnitFill + taskData.AdConstraints.AdSpanUnits
                ).ToList();
            if (breaksWithEnoughSpace.Count == 0)
            {
                return false;
            }
            int breakNum = Random.Next(breaksWithEnoughSpace.Count);
            BreakSchedule schedule = breaksWithEnoughSpace[breakNum];
            int position = Random.Next(schedule.Count + 1);
            Insert insert = new Insert()
            {
                TvBreak = schedule.BreakData,
                AdvertisementOrder = taskData.AdConstraints,
                Position = position,
                Instance = Instance,
                Solution = Solution,
            };
            insert.Execute();
            return true;
        }

        private void InsertInRandomBreak(TaskScore taskData)
        {
            int breakNum = Random.Next(Instance.Breaks.Count);
            TvBreak tvBreak = Instance.Breaks.Values.ToList()[breakNum];
            BreakSchedule schedule = Solution.AdvertisementsScheduledOnBreaks[tvBreak.ID];
            int position = Random.Next(schedule.Count + 1);
            Insert insert = new Insert()
            {
                TvBreak = schedule.BreakData,
                AdvertisementOrder = taskData.AdConstraints,
                Position = position,
                Instance = Instance,
                Solution = Solution,
            };
            insert.Execute();
        }

        public void Solve()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            ScoringFunction.AssesSolution(Solution);
            while (Solution.AdOrdersScores.Values.Any(a => !a.TimesAiredSatisfied))
            {
                foreach (AdvertisementTask advertisementOrder in Instance.AdOrders.Values)
                {
                    var taskData = Solution.AdOrdersScores[advertisementOrder.ID];
                    if (taskData.TimesAiredSatisfied)
                    {
                        continue;
                    }
                    if (!InsertInRandomNonFilledBreak(taskData))
                    {
                        InsertInRandomBreak(taskData);
                    }
                }
            }
            ScoringFunction.RecalculateSolutionScoresBasedOnTaskData(Solution);
            stopwatch.Stop();
            Solution.TimeElapsed += stopwatch.Elapsed;
        }
    }
}
