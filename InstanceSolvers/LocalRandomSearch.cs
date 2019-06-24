﻿using InstanceGenerator.InstanceData;
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
    public class LocalRandomSearch : ISolver
    {
        private Instance _instance;
        private IScoringFunction _scoringFunction;
        private Solution _solution;
        private bool _movePerformed;
        private int _seed;
        private Random _random;

        public int PositionsTakenIntoConsideration { get; set; } = 0;
        public int BreaksTakednIntoConsideration { get; set; } = 0;
        public int OrdersTakenIntoConsideration { get; set; } = 0;

        private List<TvBreak> _breakInOrder { get; set; }
        public string Description { get; set; }

        public LocalRandomSearch()
        {
            Random rnd = new Random();
            _seed = rnd.Next();
            _random = new Random(_seed);
        }

        public int Seed
        {
            get => _seed;
            set
            {
                _random = new Random(value);
                _seed = value;
            }
        }

        public Instance Instance
        {
            get
            {
                return _instance;
            }
            set
            {
                _instance = value;
                if (Solution == null)
                {
                    Solution = new Solution(Instance);
                }
            }
        }


        public IScoringFunction ScoringFunction
        {
            get => _scoringFunction;
            set
            {
                _scoringFunction = value;
                if (_scoringFunction.Instance == null)
                {
                    _scoringFunction.Instance = Instance;
                }
                if (_scoringFunction.Solution != _solution)
                {
                    _scoringFunction.Solution = _solution;
                }
                if (Solution != null && Solution.GradingFunction != ScoringFunction)
                {
                    Solution.GradingFunction = ScoringFunction;
                }
            }
        }


        public Solution Solution
        {
            get => _solution;
            set
            {
                _solution = value;
                if (_scoringFunction != null && _scoringFunction.Solution != _solution)
                {
                    _scoringFunction.Solution = _solution;
                }
            }
        }

        public void Solve()
        {
            ScoringFunction.AssesSolution(Solution);
            //due earlier are scheduled first
            //heftier are scheduled first if due at the same time
            var ordersInOrder = Instance.AdOrders.Values.OrderByDescending(order => order.AdSpanUnits).OrderBy(order => order.DueTime).ToList();
            _breakInOrder = Instance.Breaks.Values.OrderBy(b => b.StartTime).ToList();

            _movePerformed = true;
            while (Solution.CompletionScore < 1 && _movePerformed)
            {
                _movePerformed = false;
            }
        }


        private void ChooseMoveToPerform(List<Insert> moves)
        {
            foreach (var move in moves)
            {
                move.Asses();
            }
            var candidate = moves.OrderBy(m => m.OverallDifference.IntegrityLossScore).FirstOrDefault();
            if (candidate.OverallDifference.IntegrityLossScore < 0)
            {
                candidate.Execute();
                Solution.GradingFunction.RecalculateSolutionScoresBasedOnTaskData(Solution);
                _movePerformed = true;
            }
        }
    }
}
