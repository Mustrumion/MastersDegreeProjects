using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers.Solvers
{
    public class CompoundSolver : BaseSolver
    {
        public List<BaseSingleGoalHeuristic> PartialHeuristics { get; set; } = new List<BaseSingleGoalHeuristic>();
        [JsonIgnore]
        public int LoopsPerformed { get; set; }
        public int MaxLoops { get; set; } = 9999999;

        private bool TimeToEnd()
        {
            if (Solution.CompletionScore >= 1) return true;
            if (LoopsPerformed >= MaxLoops) return true;
            if (CurrentTime.Elapsed >= TimeLimit) return true;
            return false;
        }

        protected override void InternalSolve()
        {
            if(PartialHeuristics.Count == 0)
            {
                InitiatePartialSolvers();
            }
            while (!TimeToEnd())
            {
                int proportionalLimit = Convert.ToInt32(TimeLimit.TotalMilliseconds / PartialHeuristics.Count);
                foreach (var solver in PartialHeuristics)
                {
                    int limitLeft = Convert.ToInt32((TimeLimit - CurrentTime.Elapsed).TotalMilliseconds);
                    solver.Instance = Instance;
                    solver.Solution = Solution;
                    solver.TimeLimit = new TimeSpan(0, 0, 0, 0, Math.Min(proportionalLimit, limitLeft));
                    if (PropagateRandomSeed)
                    {
                        solver.Seed = Random.Next();
                    }
                    else
                    {
                        solver.Seed = (Random.Next() + new Random().Next()) % int.MaxValue;
                    }
                    solver.PropagateRandomSeed = PropagateRandomSeed;
                    solver.ScoringFunction = ScoringFunction;
                    solver.Solve();
                    Solution = solver.Solution;
                }
                LoopsPerformed += 1;
            }
        }

        private void InitiatePartialSolvers()
        {
            PartialHeuristics = new List<BaseSingleGoalHeuristic>()
            {
                new ViewsHeuristic()
                {
                    MaxBreakExtensionUnits = 40,
                    MaxInsertedPerBreak = 5,
                    MaxLoops = 1,
                },
                new BeginingsHeuristic()
                {
                    MaxBreakExtensionUnits = 70,
                    MaxLoops = 1,
                },
                new EndingsHeuristic()
                {
                    MaxBreakExtensionUnits = 100,
                    MaxLoops = 1,
                },
                new FreeSpaceHeuristic()
                {
                    MaxLoops = 1,
                },
            };
        }
    }
}
