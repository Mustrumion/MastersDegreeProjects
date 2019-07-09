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
    public class Delete : IMove
    {
        public TvBreak TvBreak { get; set; }
        public int Position { get; set; }
        public TaskCompletionDifference OverallDifference { get; set; }
        public Solution Solution { get; set; }
        public Instance Instance { get; set; }

        private Dictionary<int, TaskScore> _changedOrderStatsBefore;
        private Dictionary<int, TaskScore> _changedOrderStatsAfter;
        private Dictionary<int, TaskScore> _oldBreakScores;
        private Dictionary<int, TaskScore> _newBreakScores;
        private BreakSchedule _oldSchedule;
        private BreakSchedule _newSchedule;
        public Dictionary<int, TaskCompletionDifference> CompletionDifferences { get; set; }


        private void RollBackSolutionScores()
        {
            foreach (var taskData in _newSchedule.Scores.Values)
            {
                Solution.AdOrdersScores[taskData.TaskID].RemoveOtherDataFromThis(taskData);
            }
            foreach (var taskData in _oldSchedule.Scores.Values)
            {
                Solution.AdOrdersScores[taskData.TaskID].MergeOtherDataIntoThis(taskData);
            }
        }

        private void AddToSolutionScores()
        {
            _changedOrderStatsBefore = new Dictionary<int, TaskScore>();
            _changedOrderStatsAfter = new Dictionary<int, TaskScore>();
            CompletionDifferences = new Dictionary<int, TaskCompletionDifference>();
            foreach (var taskData in _oldSchedule.Scores.Values)
            {
                TaskScore statsCopy = new TaskScore() { AdConstraints = taskData.AdConstraints };
                TaskScore currentStatsForTask = Solution.AdOrdersScores[taskData.TaskID];
                statsCopy.OverwriteStatsWith(currentStatsForTask);
                _changedOrderStatsBefore.Add(statsCopy.TaskID, statsCopy);
                currentStatsForTask.RemoveOtherDataFromThis(taskData);
            }
            foreach (var taskData in _newSchedule.Scores.Values)
            {
                TaskScore currentStatsForTask = Solution.AdOrdersScores[taskData.TaskID];
                if (!_changedOrderStatsBefore.TryGetValue(taskData.TaskID, out TaskScore statsCopy))
                {
                    statsCopy = new TaskScore() { AdConstraints = taskData.AdConstraints };
                    statsCopy.OverwriteStatsWith(currentStatsForTask);
                    _changedOrderStatsBefore.Add(statsCopy.TaskID, statsCopy);
                }
                currentStatsForTask.MergeOtherDataIntoThis(taskData);
            }
            foreach (var statsBefore in _changedOrderStatsBefore.Values)
            {
                TaskScore statsCopy = new TaskScore() { AdConstraints = statsBefore.AdConstraints };
                statsCopy.OverwriteStatsWith(Solution.AdOrdersScores[statsBefore.TaskID]);
                _changedOrderStatsAfter.Add(statsBefore.TaskID, statsCopy);
                CompletionDifferences.Add(statsBefore.TaskID, statsCopy.CalculateDifference(statsBefore));
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
            _oldBreakScores = _oldSchedule.Scores;
            _newSchedule = _oldSchedule.DeepClone();
            _newSchedule.RemoveAt(Position);
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
            Solution.RemoveAdFromBreak(TvBreak, Position);
            Solution.AdvertisementsScheduledOnBreaks[TvBreak.ID].Scores = _newBreakScores;
            foreach (var statsAfter in _changedOrderStatsAfter.Values)
            {
                Solution.AdOrdersScores[statsAfter.TaskID].OverwriteStatsWith(statsAfter);
            }
        }

        public void RollBack()
        {
            throw new NotImplementedException();
        }

        public void CleanData()
        {
            _changedOrderStatsBefore = null;
            _changedOrderStatsAfter = null;
            _oldBreakScores = null;
            _newBreakScores = null;
            _oldSchedule = null;
            _newSchedule = null;
            CompletionDifferences = null;
        }
    }
}
