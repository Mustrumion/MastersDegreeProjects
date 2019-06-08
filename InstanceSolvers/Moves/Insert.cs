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

        private Dictionary<int, TaskData> TaskDataBeforeMove { get; set; }
        private Dictionary<int, TaskData> TaskDataAfterMove { get; set; }
        public Dictionary<int, TaskCompletionDifference> CompletionDifferences { get; set; }

        public void Asses()
        {
            var currentSchedule = Solution.AdvertisementsScheduledOnBreaks[TvBreak.ID];
            var currentBreakAssesment = Solution.GradingFunction.AssesBreak(currentSchedule);
            var orderPostMove = currentSchedule.Order.ToList();
            orderPostMove.Insert(Position, AdvertisementOrder);
            var assessmentAfterMove = Solution.GradingFunction.AssesBreak(new BreakSchedule(TvBreak) { BreakData = TvBreak, Order = orderPostMove });
            TaskDataBeforeMove = Solution.AdOrderData;
            TaskDataAfterMove = TaskDataBeforeMove.ToDictionary(a => a.Key, a => a.Value.Clone());
            foreach(var taskData in TaskDataAfterMove)
            {
                if(currentBreakAssesment.TryGetValue(taskData.Key, out var oldBreakData))
                {
                    taskData.Value.RemoveOtherDataFromThis(oldBreakData);
                }
                if(assessmentAfterMove.TryGetValue(taskData.Key, out var newBreakData))
                {
                    taskData.Value.RemoveOtherDataFromThis(newBreakData);
                }
            }
            CompletionDifferences = TaskDataAfterMove.ToDictionary(d => d.Key, d => d.Value.CalculateDifference(TaskDataBeforeMove[d.Key]));
            OverallDifference = new TaskCompletionDifference();
            foreach(var difference in CompletionDifferences)
            {
                OverallDifference.Add(difference.Value);
            }
        }

        public void Execute()
        {
            if(TaskDataAfterMove == null)
            {
                Asses();
            }
            Solution.AddAdToBreak(AdvertisementOrder, TvBreak, Position);
            Solution.AdOrderData = TaskDataAfterMove;
        }

        public void RollBack()
        {
            Solution.RemoveAdFromBreak(AdvertisementOrder, TvBreak, Position);
            Solution.AdOrderData = TaskDataBeforeMove;
        }
    }
}
