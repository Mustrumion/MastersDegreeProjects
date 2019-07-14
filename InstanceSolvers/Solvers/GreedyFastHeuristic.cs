using InstanceGenerator;
using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using InstanceSolvers.Solvers.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers.Solvers
{
    /// <summary>
    /// This heurisitc generates compatible break orders filling them with advertisements untill their times aired and views are satisfied
    /// </summary>
    public class GreedyFastHeuristic : BaseSolver, ISolver
    {
        public int MaxOverfillUnits { get; set; } = 10;
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
            ScoringFunction.RecalculateSolutionScoresBasedOnTaskData(Solution);
        }

        private void GenerateReportEntry()
        {
            Reporter.AddEntry(new ReportEntry()
            {
                Time = DateTime.Now,
                Action = "Added break",
                AttainedAcceptable = Solution.IntegrityLossScore <= 0,
                WeightedLoss = Solution.WeightedLoss,
                IntegrityLoss = Solution.IntegrityLossScore,
            });
        }

        private void CreateSchedule(BreakSchedule schedule)
        {
            var advertisementDataList = Solution.AdOrdersScores.Values.Where(t => !t.TimesAiredSatisfied || t.ViewsSatisfied).ToList();
            advertisementDataList.Shuffle(Random);
            foreach(var ad in advertisementDataList)
            {
                if (Instance.GetTypeToBreakIncompatibility(ad, schedule) == 1) {
                    continue;
                }
                if (Instance.GetBulkBrandIncompatibilities(ad.AdConstraints, schedule.Order).Contains(double.PositiveInfinity))
                {
                    continue;
                }
                schedule.AddAd(ad.AdConstraints);
                if(schedule.UnitFill > schedule.BreakData.SpanUnits + MaxOverfillUnits)
                {
                    break;
                }
            }
            ScoringFunction.AssesBreak(schedule);
            AddToSolutionScores(schedule.Scores);
            GenerateReportEntry();
        }

        protected override void InternalSolve()
        {
            var breakList = Solution.AdvertisementsScheduledOnBreaks.Values.ToList();
            breakList.Shuffle(Random);
            foreach (var schedule in breakList)
            {
                CreateSchedule(schedule);
            }
        }
    }
}
