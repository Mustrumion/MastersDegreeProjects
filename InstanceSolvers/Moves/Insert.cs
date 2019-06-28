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
        public AdvertisementOrder AdvertisementOrder { get; set; }
        public int Position { get; set; }

        private Dictionary<int, TaskData> _changedOrderStatsBefore;
        private Dictionary<int, TaskData> _changedOrderStatsAfter;
        private Dictionary<int, TaskData> _oldBreakScores;
        private Dictionary<int, TaskData> _newBreakScores;
        private BreakSchedule _oldSchedule;
        private BreakSchedule _newSchedule;
        public Dictionary<int, TaskCompletionDifference> CompletionDifferences { get; set; }

        private void RollBackSolutionScores()
        {
            foreach (var taskData in _newSchedule.Scores.Values)
            {
                Solution.AdOrderData[taskData.TaskID].RemoveOtherDataFromThis(taskData);
            }
            foreach (var taskData in _oldSchedule.Scores.Values)
            {
                Solution.AdOrderData[taskData.TaskID].MergeOtherDataIntoThis(taskData);
            }
        }

        private void AddToSolutionScores()
        {
            _changedOrderStatsBefore = new Dictionary<int, TaskData>();
            _changedOrderStatsAfter = new Dictionary<int, TaskData>();
            CompletionDifferences = new Dictionary<int, TaskCompletionDifference>();
            foreach (var taskData in _oldSchedule.Scores.Values)
            {
                TaskData statsCopy = new TaskData() { AdvertisementOrderData = taskData.AdvertisementOrderData };
                TaskData currentStatsForTask = Solution.AdOrderData[taskData.TaskID];
                statsCopy.OverwriteStatsWith(currentStatsForTask);
                _changedOrderStatsBefore.Add(statsCopy.TaskID, statsCopy);
                currentStatsForTask.RemoveOtherDataFromThis(taskData);
            }
            foreach (var taskData in _newSchedule.Scores.Values)
            {
                TaskData currentStatsForTask = Solution.AdOrderData[taskData.TaskID];
                if (!_changedOrderStatsBefore.TryGetValue(taskData.TaskID, out TaskData statsCopy))
                {
                    statsCopy = new TaskData() { AdvertisementOrderData = taskData.AdvertisementOrderData };
                    statsCopy.OverwriteStatsWith(currentStatsForTask);
                    _changedOrderStatsBefore.Add(statsCopy.TaskID, statsCopy);
                }
                currentStatsForTask.MergeOtherDataIntoThis(taskData);
            }
            foreach(var statsBefore in _changedOrderStatsBefore.Values)
            {
                TaskData statsCopy = new TaskData() { AdvertisementOrderData = statsBefore.AdvertisementOrderData };
                statsCopy.OverwriteStatsWith(Solution.AdOrderData[statsBefore.TaskID]);
                _changedOrderStatsAfter.Add(statsBefore.TaskID, statsCopy);
                CompletionDifferences.Add(statsBefore.TaskID, statsCopy.CalculateDifference(statsBefore));
            }
            OverallDifference = new TaskCompletionDifference();
            foreach (var difference in CompletionDifferences)
            {
                OverallDifference.Add(difference.Value);
            }
        }

        private void CountBreakTaskChanges()
        {
            _oldSchedule = Solution.AdvertisementsScheduledOnBreaks[TvBreak.ID];
            if(_oldSchedule.Scores == null)
            {
                Solution.GradingFunction.AssesBreak(_oldSchedule);
            }
            _oldBreakScores = _oldSchedule.Scores;
            _newSchedule = _oldSchedule.DeepClone();
            _newSchedule.Insert(Position, AdvertisementOrder);
            Solution.GradingFunction.AssesBreak(_newSchedule);
            _newBreakScores = _newSchedule.Scores;
        }

        public void Asses()
        {
            CountBreakTaskChanges();
            AddToSolutionScores();
            RollBackSolutionScores();
        }

        public void Execute()
        {
            if (OverallDifference == null)
            {
                Asses();
            }
            Solution.AddAdToBreak(AdvertisementOrder, TvBreak, Position);
            Solution.AdvertisementsScheduledOnBreaks[TvBreak.ID].Scores = _newBreakScores;
            foreach (var statsAfter in _changedOrderStatsAfter.Values)
            {
                Solution.AdOrderData[statsAfter.TaskID].OverwriteStatsWith(statsAfter);
            }
        }

        public void RollBack()
        {
            Solution.RemoveAdFromBreak(AdvertisementOrder, TvBreak, Position);
            foreach (var statsBefore in _changedOrderStatsBefore.Values)
            {
                Solution.AdOrderData[statsBefore.TaskID].OverwriteStatsWith(statsBefore);
            }
        }
    }
}
