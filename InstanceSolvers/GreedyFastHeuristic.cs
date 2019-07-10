using InstanceGenerator;
using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers
{
    /// <summary>
    /// This heurisitc generates compatible break orders filling them with advertisements untill their times aired and views are satisfied
    /// </summary>
    public class GreedyFastHeuristic : BaseSolver, ISolver
    {
        public int MaxOverfillUnits { get; set; } = 10;
        public bool DiagnosticMessages { get; set; } = false;
        private List<AdvertisementTask> _order { get; set; }

        public GreedyFastHeuristic() : base()
        {
        }
        
        private void AddToSolutionScores(Dictionary<int, TaskScore> addedScores)
        {
            foreach (var taskData in addedScores.Values)
            {
                TaskScore currentStatsForTask = Solution.AdOrdersScores[taskData.TaskID];
                currentStatsForTask.MergeOtherDataIntoThis(taskData);
            }
        }


        private void CreateSchedule(BreakSchedule schedule)
        {
            var advertisementDataList = Solution.AdOrdersScores.Values.Where(t => !t.TimesAiredSatisfied || t.ViewsSatisfied).ToList();
            advertisementDataList.Shuffle(Random);
            foreach(var ad in advertisementDataList)
            {
                if (Solution.GetTypeToBreakIncompatibility(ad, schedule) == 1) {
                    continue;
                }
                if (Solution.GetBulkBrandIncompatibilities(ad.AdConstraints, schedule.Order).Contains(double.PositiveInfinity))
                {
                    continue;
                }
                schedule.Append(ad.AdConstraints);
                if(schedule.UnitFill > schedule.BreakData.SpanUnits + MaxOverfillUnits)
                {
                    break;
                }
            }
            ScoringFunction.AssesBreak(schedule);
            AddToSolutionScores(schedule.Scores);
        }

        protected override void InternalSolve()
        {
            var breakList = Solution.AdvertisementsScheduledOnBreaks.Values.ToList();
            breakList.Shuffle(Random);
            foreach (var schedule in breakList)
            {
                CreateSchedule(schedule);
            }
            ScoringFunction.RecalculateSolutionScoresBasedOnTaskData(Solution);
        }
    }
}
