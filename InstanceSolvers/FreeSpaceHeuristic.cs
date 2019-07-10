using InstanceGenerator;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using InstanceSolvers.Moves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers
{
    public class FreeSpaceHeuristic : BaseSolver, ISolver
    {
        private bool _movePerformed;

        public int NumberOfMoves { get; set; }
        public int LoopsPerformed { get; set; }
        public int MaxLoops { get; set; } = 9999999;

        public FreeSpaceHeuristic() : base()
        {
        }

        private bool TimeToEnd()
        {
            if (Solution.CompletionScore >= 1) return true;
            if (!_movePerformed) return true;
            if (LoopsPerformed >= MaxLoops) return true;
            if (CurrentTime.Elapsed >= TimeLimit) return true;
            return false;
        }

        protected override void InternalSolve()
        {
            _movePerformed = true;
            while (!TimeToEnd())
            {
                _movePerformed = false;
                var breaks = Solution.AdvertisementsScheduledOnBreaks.Values.ToList();
                breaks.Shuffle(Random);
                foreach(var tvBreak in breaks)
                {
                    TryToCleanBreak(tvBreak);
                    if (CurrentTime.Elapsed >= TimeLimit) break;
                }
                LoopsPerformed += 1;
            }
            Solution.GradingFunction.RecalculateSolutionScoresBasedOnTaskData(Solution);
            if (DiagnosticMessages) Console.WriteLine($"Insertion heuristic ended. Number of moves: {NumberOfMoves}. LoopsPerformed: {LoopsPerformed}.");
        }

        private void TryToCleanBreak(BreakSchedule tvBreak)
        {
            var ads = tvBreak.Order.ToList();
            ads.Shuffle(Random);
            for(int pos = 0; pos < tvBreak.Count; pos++)
            {
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
                    NumberOfMoves += 1;
                    _movePerformed = true;
                }
                if (CurrentTime.Elapsed >= TimeLimit) break;
            }
        }
    }
}
