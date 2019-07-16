using InstanceGenerator;
using InstanceSolvers.Solvers.Base;
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
        public bool PassReporter { get; set; }

        protected override void PerformLoop()
        {
            if (PartialHeuristics.Count == 0) InitiatePartialSolvers();
            int proportionalLimit = Convert.ToInt32(TimeLimit.TotalMilliseconds / PartialHeuristics.Count);
            if (RandomOrder) PartialHeuristics.Shuffle(Random);
            foreach (var solver in PartialHeuristics)
            {
                int limitLeft = Convert.ToInt32(TimeLimit.TotalMilliseconds - CurrentTime.Elapsed.TotalMilliseconds);
                if (limitLeft < 0) break;
                solver.TimeLimit = new TimeSpan(0, 0, 0, 0, Math.Min(proportionalLimit, limitLeft));
                PassContextToSolver(solver);
                if(!PassReporter) solver.Reporter = new NullReporter();
                solver.Solve();
                Solution = solver.Solution;
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
