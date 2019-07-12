﻿using InstanceGenerator;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers.Solvers
{
    public class CompoundSolver : BaseGreedyTransformationHeuristic
    {
        public List<BaseGreedyTransformationHeuristic> PartialHeuristics { get; set; } = new List<BaseGreedyTransformationHeuristic>();
        public bool RandomOrder { get; set; }
        

        protected override void PerformLoop()
        {
            if (PartialHeuristics.Count == 0) InitiatePartialSolvers();
            int proportionalLimit = Convert.ToInt32(TimeLimit.TotalMilliseconds / PartialHeuristics.Count);
            if (RandomOrder) PartialHeuristics.Shuffle(Random);
            foreach (var solver in PartialHeuristics)
            {
                int limitLeft = Convert.ToInt32(TimeLimit.TotalMilliseconds - CurrentTime.Elapsed.TotalMilliseconds);
                if (limitLeft < 0) break;
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
                // revert the time increase automatically added to the solution, as this solver will add its whole time at the end
                Solution.TimeElapsed -= solver.CurrentTime.Elapsed;
                if (solver.MovePerformed)
                {
                    _numberOfMoves += solver.NumberOfMoves;
                    _movePerformed = true;
                }
            }
        }


        private void InitiatePartialSolvers()
        {
            PartialHeuristics = new List<BaseGreedyTransformationHeuristic>()
            {
                new ViewsHeuristic()
                {
                    MaxBreakExtensionUnits = 50,
                    MaxInsertedPerBreak = 5,
                    MaxLoops = 1,
                },
                new BeginingsHeuristic()
                {
                    MaxBreakExtensionUnits = 999,
                    MaxLoops = 1,
                },
                new EndingsHeuristic()
                {
                    MaxBreakExtensionUnits = 999,
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