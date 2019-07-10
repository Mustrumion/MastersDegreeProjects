using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers
{
    public abstract class BaseSingleGoalHeuristic : BaseSolver
    {
        protected bool _movePerformed;
        public int MaxLoops { get; set; } = 9999999;
        public int NumberOfMoves { get; set; }
        public int LoopsPerformed { get; set; }


        protected bool TimeToEnd()
        {
            if (Solution.CompletionScore >= 1) return true;
            if (!_movePerformed) return true;
            if (LoopsPerformed >= MaxLoops) return true;
            if (CurrentTime.Elapsed >= TimeLimit) return true;
            return false;
        }

        protected abstract void PerformLoop();

        protected override void InternalSolve()
        {
            _movePerformed = true;
            while (!TimeToEnd())
            {
                _movePerformed = false;
                PerformLoop();
                LoopsPerformed += 1;
            }
            Solution.GradingFunction.RecalculateSolutionScoresBasedOnTaskData(Solution);
            if (DiagnosticMessages) Console.WriteLine($"Beginings heuristic ended. Number of moves: {NumberOfMoves}. LoopsPerformed: {LoopsPerformed}.");
        }
    }
}
