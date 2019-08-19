using InstanceGenerator.InstanceData;
using InstanceGenerator.SolutionObjects;
using InstanceSolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers.Transformations
{
    public interface ITransformation
    {
        TaskCompletionDifference OverallDifference { get; set; }
        Solution Solution { get; set; }
        Instance Instance { get; set; }
        void Execute();
        void Asses();
        void CleanData();
        ReportEntry GenerateReportEntry();
    }
}
