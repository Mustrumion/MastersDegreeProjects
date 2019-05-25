using InstanceGenerator;
using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers
{
    public class GradingFunction : IScoringFunction
    {
        public Solution Solution { get; set; }
        public Instance Instance { get; set; }
        public string Description { get; set; }

        public void CalculateBreakDifferences(List<int> breakBefore, List<int> breakAfter, IMove moveResponsible)
        {

        }
    }
}
