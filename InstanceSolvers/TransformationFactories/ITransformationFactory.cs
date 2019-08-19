using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using InstanceSolvers.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers.TransformationFactories
{
    public interface ITransformationFactory
    {
        Random Random { get; set; }
        bool MildlyRandomOrder { get; set; }
        int Seed { get; set; }
        Instance Instance { get; set; }
        Solution Solution { get; set; }
        IEnumerable<ITransformation> GenerateMoves();
        void WidenNeighborhood(double alpha);
        void NarrowNeighborhood(double alpha);
    }
}
