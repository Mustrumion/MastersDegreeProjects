using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGenerator.SolutionObjects
{
    public class Solution
    {
        private Dictionary<int, BreakSchedule> _advertisementsScheduledOnBreaks = new Dictionary<int, BreakSchedule>();
        private IScoringFunction _gradingFunction;

        [JsonIgnore]
        public Instance Instance { get; set; }

        [JsonIgnore]
        public IScoringFunction GradingFunction
        {
            get => _gradingFunction;
            set
            {
                if(_gradingFunction == value)
                {
                    return;
                }
                _gradingFunction = value;
                _gradingFunction.Instance = Instance;
                _gradingFunction.Solution = this;
                foreach (var taskScore in AdOrdersScores.Values)
                {
                    taskScore.ScoringFunction = value;
                }
            }
        }

        /// <summary>
        /// Dictionary of lists. Dictionary keys represent break IDs. Lists contain job objects in order scheduled for a break given by the key.
        /// </summary>
        [JsonIgnore]
        public Dictionary<int, BreakSchedule> AdvertisementsScheduledOnBreaks
        {
            get => _advertisementsScheduledOnBreaks;
            set
            {
                _advertisementsScheduledOnBreaks = value;
                RestoreTaskView();
            }
        }

        /// <summary>
        /// Human readable description.
        /// </summary>
        [Description("Human readable description.")]
        public string Description { get; set; }

        /// <summary>
        /// Time spent generating the solution.
        /// </summary>
        [Description("Time spent generating the solution.")]
        public TimeSpan TimeElapsed { get; set; }

        /// <summary>
        /// Overall solution score.
        /// </summary>
        [Description("Overall solution score.")]
        public double WeightedLoss { get; set; }

        /// <summary>
        /// Loss from late ad contract completion.
        /// </summary>
        [Description("Loss from late ad contract completion.")]
        public double OverdueAdsLoss { get; set; }

        /// <summary>
        /// Loss form scheduling soft-incompatible ads on the same break.
        /// </summary>
        [Description("Loss form scheduling soft-incompatible ads on the same break.")]
        public double MildIncompatibilityLoss { get; set; }

        /// <summary>
        /// Loss form overextending breaks.
        /// </summary>
        [Description("Loss form overextending breaks.")]
        public double ExtendedBreakLoss { get; set; }

        /// <summary>
        /// Detailed integrity score. Subjective, depends on grading object.
        /// </summary>
        [Description("Detailed integrity score.")]
        public double IntegrityLossScore { get; set; }

        /// <summary>
        /// Number of advertisement orders (tasks) with hard constraints met.
        /// </summary>
        [Description("Number of advertisement orders (tasks) with hard constraints met.")]
        public int Completion { get; set; }

        /// <summary>
        /// Total stats for the solution.
        /// </summary>
        [Description("Total stats for the solution.")]
        public TasksStats TotalStats { get; set; }

        /// <summary>
        /// Dictionary of task stats and where in the solution are instances of the tasks. Key - ID of the order (task).
        /// </summary>
        [Description("Dictionary of task stats and where in the solution are instances of the tasks. Key - ID of the order (task).")]
        [JsonProperty(Order = 1)]
        public Dictionary<int, TaskScore> AdOrdersScores { get; set; } = new Dictionary<int, TaskScore>();

        /// <summary>
        /// Simplified view on the current solution state. Created for serialization purpose. 
        /// Dictionary of lists. Dictionary keys represent break IDs. Lists contain job IDs in order scheduled for a break given by the key.
        /// </summary>
        [JsonProperty(Order = 2)]
        [Description("Dictionary of lists. Dictionary keys represent break IDs. Lists contain job IDs in order scheduled for a break given by the key.")]
        public Dictionary<int, List<int>> AdvertisementIdsScheduledOnBreaks { get; set; } = new Dictionary<int, List<int>>();


        public Solution(){}

        public Solution(Instance instance) : base() 
        {
            Instance = instance;
            _advertisementsScheduledOnBreaks = new Dictionary<int, BreakSchedule>();
            foreach (var tvBreak in Instance.Channels.Values.SelectMany(c => c.Breaks))
            {
                _advertisementsScheduledOnBreaks.Add(tvBreak.ID, new BreakSchedule(tvBreak));
            }
        }


        /// <summary>
        /// Fraction of tasks with hard constraints met.
        /// </summary>
        [Description("Fraction of tasks with hard constraints met.")]
        public double CompletionScore
        {
            get
            {
                return (double)Completion / MaxCompletion;
            }
        }

        /// <summary>
        /// Number of tasks.
        /// </summary>
        [Description("Number of tasks.")]
        public int MaxCompletion
        {
            get
            {
                return Instance.AdOrders.Count;
            }
        }

        public string GradingFunctionDescription
        {
            get
            {
                return GradingFunction?.Description;
            }
        }

        public bool Scored { get; set; }

        public void GenerateSolutionFromRealData()
        {
            _advertisementsScheduledOnBreaks = new Dictionary<int, BreakSchedule>();
            foreach (var tvBreak in Instance.Channels.Values.SelectMany(c => c.Breaks))
            {
                _advertisementsScheduledOnBreaks.Add(tvBreak.ID, 
                        new BreakSchedule(tvBreak, tvBreak.Advertisements.Select(a => a.AdvertisementOrder).ToList()));
            }
        }

        public void PrepareForSerialization()
        {
            AdvertisementIdsScheduledOnBreaks = _advertisementsScheduledOnBreaks.ToDictionary(
                b => b.Key,
                b => b.Value.GetOrderIdsCopy());
        }

        public void RestoreStructures()
        {
            _advertisementsScheduledOnBreaks = AdvertisementIdsScheduledOnBreaks.ToDictionary(
                b => b.Key,
                b => new BreakSchedule(Instance.Breaks[b.Key], b.Value.Select(a => Instance.AdOrders[a]).ToList()));
            RestoreTaskView();
        }

        public void RestoreTaskView()
        {
            AdOrdersScores = new Dictionary<int, TaskScore>();
            foreach (var task in Instance.AdOrders.Values)
            {
                AdOrdersScores.Add(task.ID, new TaskScore() { AdConstraints = task, ScoringFunction = GradingFunction});
            }
        }
        


        /// <summary>
        /// Adds an advertisement instance to the solution.
        /// Does not update scores.
        /// </summary>
        /// <param name="ad"></param>
        /// <param name="tvBreak"></param>
        /// <param name="position"></param>
        public void AddAdToBreak(AdvertisementTask ad, TvBreak tvBreak, int position)
        {
            if(!_advertisementsScheduledOnBreaks.TryGetValue(tvBreak.ID, out var breakSchedule))
            {
                breakSchedule = new BreakSchedule(tvBreak);
            }
            var adsInBreak = breakSchedule.Order;
            breakSchedule.Insert(position, ad);
            breakSchedule.Scores = null;
        }

        /// <summary>
        /// Removes an advertisement instance from the solution.
        /// </summary>
        /// <param name="ad"></param>
        /// <param name="tvBreak"></param>
        /// <param name="position"></param>
        public void RemoveAdFromBreak(TvBreak tvBreak, int position)
        {
            var schedule = _advertisementsScheduledOnBreaks[tvBreak.ID];
            var adsInBreak = schedule.Order;
            //Move positions by one in helper structure for ads after this one.
            schedule.RemoveAt(position);
        }


        public Solution TakeSnapshot()
        {
            PrepareForSerialization();
            Solution solution = new Solution()
            {
                Instance = Instance,
                AdvertisementIdsScheduledOnBreaks = AdvertisementIdsScheduledOnBreaks,
                TotalStats = TotalStats,
                Completion = Completion,
                IntegrityLossScore = IntegrityLossScore,
                ExtendedBreakLoss = ExtendedBreakLoss,
                MildIncompatibilityLoss = MildIncompatibilityLoss,
                OverdueAdsLoss = OverdueAdsLoss,
                WeightedLoss = WeightedLoss,
                GradingFunction = GradingFunction.GetAnotherOne(),
            };
            return solution;
        }


        public bool IsBetterThan(Solution solution)
        {
            if (solution == null) return true;
            if (IntegrityLossScore < solution.IntegrityLossScore) return true;
            if (IntegrityLossScore == solution.IntegrityLossScore && WeightedLoss < solution.WeightedLoss) return true;
            return false;
        }

        public Solution DeepCopy()
        {
            return new Solution()
            {
                Instance = Instance,
                AdvertisementsScheduledOnBreaks = AdvertisementsScheduledOnBreaks.ToDictionary(a => a.Key, a => a.Value.DeepClone()),
                AdOrdersScores = AdOrdersScores.ToDictionary(a => a.Key, a => a.Value.Clone()),
                TimeElapsed = TimeElapsed,
                Description = Description,
                Completion = Completion,
                Scored = Scored,
                IntegrityLossScore = IntegrityLossScore,
                ExtendedBreakLoss = ExtendedBreakLoss,
                MildIncompatibilityLoss = MildIncompatibilityLoss,
                OverdueAdsLoss = OverdueAdsLoss,
                WeightedLoss = WeightedLoss,
                TotalStats = TotalStats,
                GradingFunction = GradingFunction.GetAnotherOne(),
            };
        }

        public void AddToSolutionScores(Dictionary<int, TaskScore> addedScores)
        {
            foreach (var taskData in addedScores.Values)
            {
                TaskScore currentStatsForTask = AdOrdersScores[taskData.ID];
                currentStatsForTask.MergeOtherDataIntoThis(taskData);
            }
        }

        public void RemoveFromSolutionScores(Dictionary<int, TaskScore> removedScores)
        {
            foreach (var taskData in removedScores.Values)
            {
                TaskScore currentStatsForTask = AdOrdersScores[taskData.ID];
                currentStatsForTask.RemoveOtherDataFromThis(taskData, null);
            }
        }
    }
}
