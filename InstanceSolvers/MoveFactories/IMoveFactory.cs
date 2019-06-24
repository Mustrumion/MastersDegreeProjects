using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers.MoveFactories
{
    public interface IMoveFactory
    {
        Random Random { get; set; }
        bool MildlyRandomOrder { get; set; }
        Instance Instance { get; set; }
        Solution Solution { get; set; }
        IEnumerable<IMove> GenerateMoves();
    }
}
