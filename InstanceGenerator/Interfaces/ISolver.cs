﻿using InstanceGenerator.InstanceData;
using InstanceGenerator.SolutionObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGenerator.Interfaces
{
    public interface ISolver
    {
        string Description { get; set; }
        int Seed { get; set; }
        Solution Solution { get; set; }
        IScoringFunction ScoringFunction { get; set; }
        Instance Instance { get; set; }
        void Solve();
    }
}