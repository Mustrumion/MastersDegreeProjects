using Newtonsoft.Json;
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
        protected int _loopsPerformed;
        protected int _numberOfMoves;

        public int MaxLoops { get; set; } = 9999999;
        public int NumberOfMoves { get => _numberOfMoves; }
        [JsonIgnore]
        public int LoopsPerformed { get => _loopsPerformed; }


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
                _loopsPerformed += 1;
            }
            Solution.GradingFunction.RecalculateSolutionScoresBasedOnTaskData(Solution);
            if (DiagnosticMessages) Console.WriteLine($"Heuristic ended. Number of moves: {NumberOfMoves}. LoopsPerformed: {LoopsPerformed}.");
        }
    }
}
