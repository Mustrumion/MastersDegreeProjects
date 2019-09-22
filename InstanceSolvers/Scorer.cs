using InstanceGenerator;
using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers
{
    public class Scorer : IScoringFunction
    {
        public double OverdueTasksLossWeight { get; set; } = 0.0001;
        public double BreakExtensionLossWeight { get; set; } = 1;
        public double MildIncompatibilityLossWeight { get; set; } = 0.000001;

        private Solution _solution;
        [JsonIgnore]
        public Instance Instance { get; set; }
        public string Description
        {
            get =>
$@"Grading function 0.1.0. 
Task delay loss is counted as a task weight multiplied by delay in days (rounded up).
Break extension loss is counted as break extension in seconds.
Soft incompatibility between owners of the same advertisement is a sum of occurece weights multiplied by the corresponding tasks weights.
Weights for the loss function components:
OverdueTasksLossWeight = {OverdueTasksLossWeight}
BreakExtensionLossWeight = {BreakExtensionLossWeight}
MildIncompatibilityLossWeight = {MildIncompatibilityLossWeight}";
        }


        [JsonIgnore]
        public Solution Solution
        {
            get => _solution;
            set
            {
                if(_solution == value || value == null)
                {
                    return;
                }
                _solution = value;
                _solution.GradingFunction = this;
            }
        }


        public void AssesBreak(BreakSchedule schedule)
        {
            BreakScorer breakScorer = new BreakScorer()
            {
                Instance = Instance,
                ScoringFunction = this
            };
            breakScorer.AssesBreak(schedule);
        }


        public void AssesSolution(Solution solution)
        {
            Solution = solution;
            Dictionary<int, TaskScore> statsData = Instance.AdOrders.ToDictionary(
                    a => a.Key,
                    a => new TaskScore() { AdConstraints = a.Value, ScoringFunction = this });
            foreach (var taskData in statsData.Values)
            {
                taskData.RecalculateLoss();
            }
            foreach (var tvBreak in Solution.AdvertisementsScheduledOnBreaks.Values)
            {
                if (tvBreak.Scores == null)
                {
                    AssesBreak(tvBreak);
                }
                Dictionary<int, TaskScore> dataToMerge = tvBreak.Scores;
                foreach (var taskData in dataToMerge)
                {
                    statsData[taskData.Key].MergeOtherDataIntoThis(taskData.Value);
                }
            }
            solution.AdOrdersScores = statsData;
            RecalculateSolutionScoresBasedOnTaskData(solution);
        }


        public void RecalculateSolutionScoresBasedOnTaskData(Solution solution)
        {
            solution.Completion = 0;
            solution.WeightedLoss = 0;
            solution.IntegrityLossScore = 0;
            solution.ExtendedBreakLoss = 0;
            solution.MildIncompatibilityLoss = 0;
            solution.OverdueAdsLoss = 0;
            solution.TotalStats = new TasksStats();
            foreach (var taskScore in solution.AdOrdersScores.Values)
            {
                solution.Completion += taskScore.Completed ? 1 : 0;
                solution.WeightedLoss += taskScore.WeightedLoss;
                solution.IntegrityLossScore += taskScore.IntegrityLossScore;
                solution.ExtendedBreakLoss += taskScore.ExtendedBreakLoss;
                solution.MildIncompatibilityLoss += taskScore.MildIncompatibilityLoss;
                solution.OverdueAdsLoss += taskScore.OverdueAdsLoss;
                solution.TotalStats.AddTaskData(taskScore);
            }
            solution.Scored = true;
        }


        public void RecalculateOverdueLoss(TaskScore taskData)
        {
            var timeDifference = taskData.LastAdTime - taskData.AdConstraints.DueTime;
            if (timeDifference.TotalMilliseconds > 0)
            {
                taskData.OverdueAdsLoss = Math.Ceiling(timeDifference.TotalDays) * taskData.AdConstraints.Gain / 10;
            }
            else
            {
                taskData.OverdueAdsLoss = 0;
            }
        }

        public void RecalculateMildIncompatibilityLoss(TaskScore taskData)
        {
            taskData.MildIncompatibilityLoss = taskData.MildIncompatibilitySumOfOccurenceWeights * taskData.AdConstraints.Gain;
        }

        public void RecalculateExtendedBreakLoss(TaskScore taskData)
        {
            taskData.ExtendedBreakLoss = taskData.ExtendedBreakUnits;
        }

        public void RecalculateWeightedLoss(TaskScore taskData)
        {
            taskData.WeightedLoss = taskData.ExtendedBreakLoss * BreakExtensionLossWeight
                + taskData.OverdueAdsLoss * OverdueTasksLossWeight
                + taskData.MildIncompatibilityLoss * MildIncompatibilityLossWeight;
        }

        public void RecalculateIntegrityLoss(TaskScore taskData)
        {
            taskData.IntegrityLossScore =
                1 - taskData.StartsCompletion
                + (taskData.StartsSatisfied ? 0 : 0.01)
                + 1 - taskData.EndsCompletion
                + (taskData.EndsSatisfied ? 0 : 0.01)
                + 1 - taskData.ViewsCompletion
                + (taskData.ViewsSatisfied ? 0 : 0.01)
                + 1 - taskData.TimesAiredCompletion
                + (taskData.TimesAiredSatisfied ? 0 : 0.01)
                + taskData.SelfIncompatibilityConflictsProportion
                + taskData.SelfSpacingConflictsProportion
                + taskData.OwnerConflictsProportion
                + taskData.BreakTypeConflictsProportion;
        }

        public void RecalculateLastAdTime(TaskScore taskData, BreakSchedule tvBreak)
        {
            DateTime currentMax = DateTime.MinValue;

            if(tvBreak == null)
            {
                foreach (TaskScore breakData in Solution.AdvertisementsScheduledOnBreaks.Select(b =>
                {
                    TaskScore taskScore = null;
                    b.Value.Scores.TryGetValue(taskData.ID, out taskScore);
                    return taskScore;
                }))
                {
                    if (breakData == null)
                    {
                        continue;
                    }
                    currentMax = currentMax >= breakData.LastAdTime ? currentMax : breakData.LastAdTime;
                }
            }
            else
            {
                int i = 0;
                foreach (var breakData in tvBreak.Order)
                {
                    int endTime = tvBreak.EndTimes[i];
                    var time = tvBreak.BreakData.StartTime.AddSeconds(endTime * Instance.UnitSizeInSeconds);
                    currentMax = currentMax >= time ? currentMax : time;
                    i += 1;
                }
            }
            taskData.LastAdTime = currentMax;
        }

        /// <summary>
        /// Generate new scorer for use in parallel
        /// </summary>
        /// <returns></returns>
        public IScoringFunction GetAnotherOne()
        {
            return new Scorer()
            {
                BreakExtensionLossWeight = BreakExtensionLossWeight,
                MildIncompatibilityLossWeight = MildIncompatibilityLossWeight,
                OverdueTasksLossWeight = OverdueTasksLossWeight
            };
        }
    }
}
