using InstanceGenerator.InstanceData;
using InstanceGenerator.SolutionObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGenerator.Interfaces
{
    public interface IMove
    {
        TaskCompletionDifference OverallDifference { get; set; }
        Solution Solution { get; set; }
        Instance Instance { get; set; }
        void Execute();
        void RollBack();
        void Asses();
        void CleanData();
    }
}
