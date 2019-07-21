using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers.Moves
{
    public class Insert : IMove
    {
        public TaskCompletionDifference OverallDifference { get; set; }
        public Solution Solution { get; set; }
        public Instance Instance { get; set; }

        public TvBreak TvBreak { get; set; }
        public AdvertisementTask AdvertisementOrder { get; set; }
        public int Position { get; set; }
        
        private Dictionary<int, TaskScore> _changedOrderStatsAfter;
        private BreakSchedule _oldSchedule;
        private BreakSchedule _newSchedule;
        private Dictionary<int, TaskScore> _oldBreakScores;
        private Dictionary<int, TaskScore> _newBreakScores;
        public Dictionary<int, TaskCompletionDifference> CompletionDifferences { get; set; }


        private void AddToSolutionScores()
        {
            _changedOrderStatsAfter = new Dictionary<int, TaskScore>();
            CompletionDifferences = new Dictionary<int, TaskCompletionDifference>();
            var changedIds = _oldBreakScores.Keys.Union(_newBreakScores.Keys).ToList();
            foreach (int id in changedIds)
            {
                _changedOrderStatsAfter.Add(id, Solution.AdOrdersScores[id].Clone());
            }
            foreach (var taskData in _oldBreakScores.Values)
            {
                _changedOrderStatsAfter[taskData.ID].RemoveOtherDataFromThis(taskData);
            }
            foreach (var taskData in _newBreakScores.Values)
            {
                _changedOrderStatsAfter[taskData.ID].MergeOtherDataIntoThis(taskData);
            }
            foreach (var taskData in _changedOrderStatsAfter.Values)
            {
                CompletionDifferences.Add(taskData.ID, taskData.CalculateDifference(Solution.AdOrdersScores[taskData.ID]));
            }
            OverallDifference = new TaskCompletionDifference();
            foreach (var difference in CompletionDifferences.Values)
            {
                OverallDifference.Add(difference);
            }
        }

        private void CountBreakTaskChanges()
        {
            _oldSchedule = Solution.AdvertisementsScheduledOnBreaks[TvBreak.ID];
            if (_oldSchedule.Scores == null)
            {
                Solution.GradingFunction.AssesBreak(_oldSchedule);
            }
            _oldBreakScores = _oldSchedule.Scores.ToDictionary(s => s.Key, s => s.Value);
            _newSchedule = _oldSchedule.DeepClone();
            _newSchedule.Insert(Position, AdvertisementOrder);
            Solution.GradingFunction.AssesBreak(_newSchedule);
            _newBreakScores = _newSchedule.Scores.ToDictionary(s => s.Key, s => s.Value);
        }

        public void Asses()
        {
            CountBreakTaskChanges();
            AddToSolutionScores();
        }

        public void Execute()
        {
            if (OverallDifference == null)
            {
                Asses();
            }
            Solution.AddAdToBreak(AdvertisementOrder, TvBreak, Position);
            Solution.AdvertisementsScheduledOnBreaks[TvBreak.ID].Scores = _newSchedule.Scores;
            foreach (var statsAfter in _changedOrderStatsAfter.Values)
            {
                Solution.AdOrdersScores[statsAfter.ID].OverwriteStatsWith(statsAfter);
            }
            Solution.GradingFunction.RecalculateSolutionScoresBasedOnTaskData(Solution);
        }

        public void CleanData()
        {
            _changedOrderStatsAfter = null;
            _oldBreakScores = null;
            _newBreakScores = null;
            _oldSchedule = null;
            _newSchedule = null;
            CompletionDifferences = null;
        }

        public ReportEntry GenerateReportEntry()
        {
            return new ReportEntry()
            {
                Time = DateTime.Now,
                Action = "Insert",
                AttainedAcceptable = OverallDifference.IntegrityLossScore < 0 && Solution.CompletionScore >= 1,
                IntegrityLoss = Solution.IntegrityLossScore,
                WeightedLoss = Solution.WeightedLoss,
            };
        }
    }
}
