using InstanceGenerator;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using InstanceSolvers.Moves;
using InstanceSolvers.Solvers.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers.Solvers
{
    public class FreeSpaceHeuristic : BaseGreedyTransformationHeuristic, ISolver
    {
        public bool DeleteOnlyOverfull { get; set; }

        public FreeSpaceHeuristic() : base()
        {
        }

        protected override void PerformLoop()
        {
            var breaks = Solution.AdvertisementsScheduledOnBreaks.Values.ToList();
            if (DeleteOnlyOverfull) breaks = breaks.Where(b => b.UnitFill <= b.BreakData.SpanUnits).ToList();
            breaks.Shuffle(Random);
            foreach (var tvBreak in breaks)
            {
                TryToCleanBreak(tvBreak);
                if (CurrentTime.Elapsed >= TimeLimit) break;
            }
        }

        private void TryToCleanBreak(BreakSchedule tvBreak)
        {
            var ads = tvBreak.Order.ToList();
            ads.Shuffle(Random);
            for(int pos = 0; pos < tvBreak.Count; pos++)
            {
                if (DeleteOnlyOverfull && tvBreak.UnitFill <= tvBreak.BreakData.SpanUnits)
                {
                    break;
                }
                Delete delete = new Delete()
                {
                    Solution = Solution,
                    TvBreak = tvBreak.BreakData,
                    Position = pos,
                };
                delete.Asses();
                if (delete.OverallDifference.HasScoreImproved() && !delete.OverallDifference.AnyCompatibilityIssuesIncreased())
                {
                    delete.Execute();
                    Reporter.AddEntry(delete.GenerateReportEntry());
                    _numberOfMoves += 1;
                    _movePerformed = true;
                }
                if (CurrentTime.Elapsed >= TimeLimit) break;
            }
        }
    }
}
