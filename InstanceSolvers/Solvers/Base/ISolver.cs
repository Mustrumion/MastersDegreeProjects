﻿using InstanceGenerator.InstanceData;
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
    public interface ISolver
    {
        string Description { get; set; }
        int Seed { get; set; }
        bool PropagateRandomSeed { get; set; }
        List<ISolver> InitialSolvers { get; set; }
        Solution Solution { get; set; }
        IScoringFunction ScoringFunction { get; set; }
        Instance Instance { get; set; }
        Random Random { get; set; }
        TimeSpan TimeLimit { get; set; }
        Stopwatch CurrentTime { get; }
        IReporter Reporter { get; set; }
        bool ReportStarts { get; set; }
        bool ReportEnds { get; set; }
        void Solve();
    }
}
