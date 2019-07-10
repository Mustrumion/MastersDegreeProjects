using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers
{
    public abstract class BaseSolver : ISolver
    {
        protected Instance _instance;
        protected int _seed;
        protected IScoringFunction _scoringFunction;
        protected Solution _solution;

        public BaseSolver()
        {
            Random rnd = new Random();
            _seed = rnd.Next();
            Random = new Random(_seed);
        }

        [JsonIgnore]
        public Random Random { get; set; }
        public TimeSpan TimeLimit { get; set; } = new TimeSpan(100, 0, 0);
        [JsonIgnore]
        public Stopwatch CurrentTime { get; set; }
        public bool PropagateRandomSeed { get; set; }
        public List<ISolver> InitialSolvers { get; set; } = new List<ISolver>();
        public string Description { get; set; }

        public int Seed
        {
            get => _seed;
            set
            {
                Random = new Random(value);
                _seed = value;
            }
        }

        [JsonIgnore]
        public Instance Instance
        {
            get
            {
                return _instance;
            }
            set
            {
                if (_instance == value || value == null)
                {
                    return;
                }
                _instance = value;
                if (Solution == null || Solution.Instance != Instance)
                {
                    Solution = new Solution(Instance);
                }
                if (ScoringFunction != null)
                {
                    ScoringFunction.Instance = Instance;
                }
            }
        }

        
        public IScoringFunction ScoringFunction
        {
            get => _scoringFunction;
            set
            {
                if(_scoringFunction == value || value == null)
                {
                    return;
                }
                _scoringFunction = value;
                _scoringFunction.Instance = _instance;
                _scoringFunction.Solution = _solution;
            }
        }

        [JsonIgnore]
        public Solution Solution
        {
            get => _solution;
            set
            {
                if(_solution == value || value == null)
                {
                    return;
                }
                _solution = value;
                if (_scoringFunction != null)
                {
                    _scoringFunction.Solution = _solution;
                }
            }
        }


        private void FireInitialSolvers()
        {
            foreach (var solver in InitialSolvers)
            {
                solver.Instance = Instance;
                solver.Solution = Solution;
                if (PropagateRandomSeed)
                {
                    solver.Seed = Random.Next();
                }
                solver.PropagateRandomSeed = PropagateRandomSeed;
                solver.ScoringFunction = ScoringFunction;
                solver.Solve();
                Solution = solver.Solution;
            }
        }

        public void Solve()
        {
            if (!Solution.Scored)
            {
                ScoringFunction.AssesSolution(Solution);
            }
            FireInitialSolvers();
            CurrentTime = new Stopwatch();
            CurrentTime.Start();
            InternalSolve();
            CurrentTime.Stop();
            Solution.TimeElapsed += CurrentTime.Elapsed;
        }

        protected abstract void InternalSolve();
    }
}
