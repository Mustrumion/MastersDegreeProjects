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

namespace InstanceSolvers.Solvers.Base
{
    public abstract class BaseSolver : ISolver
    {
        protected Instance _instance;
        protected int _seed;
        protected IScoringFunction _scoringFunction;
        protected Solution _solution;
        private Stopwatch _currentTime;
        private TimeSpan _previousSolutionTime;

        public BaseSolver()
        {
            Random rnd = new Random();
            _seed = rnd.Next();
            Random = new Random(_seed);
        }

        [JsonIgnore]
        public IReporter Reporter { get; set; } = new NullReporter();
        public Random Random { get; set; }
        public TimeSpan TimeLimit { get; set; } = new TimeSpan(100, 0, 0);
        [JsonIgnore]
        public Stopwatch CurrentTime { get => _currentTime; }
        public bool PropagateRandomSeed { get; set; }
        public List<ISolver> InitialSolvers { get; set; } = new List<ISolver>();
        public string Description { get; set; }
        public bool DiagnosticMessages { get; set; }

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
                if (_scoringFunction == value || value == null)
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
                if (_solution == value || value == null)
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


        protected void PassContextToSolver(ISolver solver)
        {
            solver.Instance = Instance;
            solver.Solution = Solution;
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
            solver.Reporter = Reporter;
        }


        private void FireInitialSolvers()
        {
            foreach (var solver in InitialSolvers)
            {
                PassContextToSolver(solver);
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
            AddStartReport();
            _previousSolutionTime = Solution.TimeElapsed;
            _currentTime = new Stopwatch();
            _currentTime.Start();
            InternalSolve();
            CurrentTime.Stop();
            Solution.TimeElapsed = CurrentTime.Elapsed + _previousSolutionTime;
            AddEndReport();
        }

        private void AddStartReport()
        {
            Reporter.AddEntry(new ReportEntry()
            {
                Time = DateTime.Now,
                Action = $"Started {this.GetType().Name}",
                AttainedAcceptable = false,
                IntegrityLoss = Solution.IntegrityLossScore,
                WeightedLoss = Solution.WeightedLoss,
            });
        }

        private void AddEndReport()
        {
            Reporter.AddEntry(new ReportEntry()
            {
                Time = DateTime.Now,
                Action = $"Ended {this.GetType().Name}",
                AttainedAcceptable = false,
                IntegrityLoss = Solution.IntegrityLossScore,
                WeightedLoss = Solution.WeightedLoss,
            });
        }

        protected abstract void InternalSolve();
    }
}
