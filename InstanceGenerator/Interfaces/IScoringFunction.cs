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
        void AssesBreak(BreakSchedule schedule);
        void AssesSolution(Solution solution);
        void RecalculateSolutionScoresBasedOnTaskData(Solution solution);
        
        void RecalculateOverdueLoss(TaskScore taskData);
        void RecalculateMildIncompatibilityLoss(TaskScore taskData);
        void RecalculateExtendedBreakLoss(TaskScore taskData);
        void RecalculateWeightedLoss(TaskScore taskData);
        void RecalculateIntegrityLoss(TaskScore taskData);
        void RecalculateLastAdTime(TaskScore taskData);

        /// <summary>
        /// Since IScoringFunction by is associated with a solution it's assessing bad things may happen if it's executed in parallel on different solutions.
        /// For this purpose get other scorer object to asses multiple solutions in parallel.
        /// </summary>
        /// <returns>Another scorer, ready to score other things.</returns>
        IScoringFunction GetAnotherOne();
    }
}
