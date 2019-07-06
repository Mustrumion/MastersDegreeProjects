﻿using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers
{
    public class BaseSolver
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

        public Random Random { get; set; }

        public int Seed
        {
            get => _seed;
            set
            {
                Random = new Random(value);
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
    }
}
