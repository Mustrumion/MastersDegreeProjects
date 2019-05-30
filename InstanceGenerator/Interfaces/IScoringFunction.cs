using InstanceGenerator.InstanceData;
using InstanceGenerator.SolutionObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGenerator.Interfaces
{
    public interface IScoringFunction
    {
        Solution Solution { get; set; }
        Instance Instance { get; set; }
        string Description { get; }

        /// <summary>
        /// Function to grade a break. 
        /// </summary>
        /// <param name="orderedAds">list of order IDs in order they are aired</param>
        /// <param name="tvBreak">problem data for the break</param>
        /// <returns>assesment for each task present in the sequence of advertisements</returns>
        Dictionary<int, TaskData> AssesBreak(List<int> orderedAds, TvBreak tvBreak);
        
        void AssesSolution(Solution solution);
        void RecalculateOverdueLoss(TaskData taskData);
        void RecalculateMildIncompatibilityLoss(TaskData taskData);
        void RecalculateExtendedBreakLoss(TaskData taskData);
        void RecalculateWeightedLoss(TaskData taskData);
        void RecalculateIntegrityLoss(TaskData taskData);
    }
}
