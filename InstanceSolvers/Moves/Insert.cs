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

        private Dictionary<int, TaskData> _changedOrderStatsBefore { get; set; }
        private Dictionary<int, TaskData> _changedOrderStatsAfter { get; set; }
        private Dictionary<int, TaskData> _currentBreakAssesment { get; set; }
        private Dictionary<int, TaskData> _afterMoveAssesment { get; set; }
        public Dictionary<int, TaskCompletionDifference> CompletionDifferences { get; set; }

        private void RollBackSolutionScores()
        {
            foreach (var taskData in _afterMoveAssesment.Values)
            {
                Solution.AdOrderData[taskData.TaskID].RemoveOtherDataFromThis(taskData);
            }
            foreach (var taskData in _currentBreakAssesment.Values)
            {
                Solution.AdOrderData[taskData.TaskID].MergeOtherDataIntoThis(taskData);
            }
        }

        private void AddToSolutionScores()
        {
            _changedOrderStatsBefore = new Dictionary<int, TaskData>();
            _changedOrderStatsAfter = new Dictionary<int, TaskData>();
            CompletionDifferences = new Dictionary<int, TaskCompletionDifference>();
            foreach (var taskData in _currentBreakAssesment.Values)
            {
                TaskData statsCopy = new TaskData() { AdvertisementOrderData = taskData.AdvertisementOrderData };
                statsCopy.OverwriteStatsWith(taskData);
                _changedOrderStatsBefore.Add(statsCopy.TaskID, statsCopy);
                Solution.AdOrderData[taskData.TaskID].RemoveOtherDataFromThis(taskData);
            }
            foreach (var taskData in _afterMoveAssesment.Values)
            {
                if(!_changedOrderStatsBefore.TryGetValue(taskData.TaskID, out TaskData statsCopy))
                {
                    statsCopy = new TaskData() { AdvertisementOrderData = taskData.AdvertisementOrderData };
                    statsCopy.OverwriteStatsWith(taskData);
                    _changedOrderStatsBefore.Add(statsCopy.TaskID, statsCopy);
                }
                Solution.AdOrderData[taskData.TaskID].MergeOtherDataIntoThis(taskData);
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
            var currentSchedule = Solution.AdvertisementsScheduledOnBreaks[TvBreak.ID];
            _currentBreakAssesment = Solution.GradingFunction.AssesBreak(currentSchedule);
            var orderPostMove = currentSchedule.Order.ToList();
            orderPostMove.Insert(Position, AdvertisementOrder);
            _afterMoveAssesment = Solution.GradingFunction.AssesBreak(new BreakSchedule(TvBreak) { BreakData = TvBreak, Order = orderPostMove });
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
