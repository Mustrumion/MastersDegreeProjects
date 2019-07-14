using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers.Solvers.Base
{
    public abstract class BaseGreedyTransformationHeuristic : BaseSolver
    {
        protected bool _movePerformed;
        protected int _loopsPerformed;
        protected int _numberOfMoves;

        public int MaxLoops { get; set; } = 9999999;
        public int NumberOfMoves { get => _numberOfMoves; }
        [JsonIgnore]
        public int LoopsPerformed { get => _loopsPerformed; }
        [JsonIgnore]
        public bool MovePerformed { get => _movePerformed; }


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
            _loopsPerformed = 0;
            _numberOfMoves = 0;
            _movePerformed = true;
            while (!TimeToEnd())
            {
                _movePerformed = false;
                PerformLoop();
                _loopsPerformed += 1;
            }
            if (DiagnosticMessages) Console.WriteLine($"Heuristic ended. Number of moves: {NumberOfMoves}. LoopsPerformed: {LoopsPerformed}.");
        }
    }
}
