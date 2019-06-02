using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers.Moves
{
    public class InsertAtStart : IMove
    {
        public int IntegrityLossChange { get; set; }
        public double WeightedLossChange { get; set; }
        public Instance Instance { get; set; }
        public Solution Solution { get; set; }

        public AdvertisementOrder AdvertisementOrder { get; set; }
        public TvBreak TvBreak { get; set; }

        public void Asses()
        {
        }

        public void Execute()
        {
        }

        public void RollBack()
        {
            throw new NotImplementedException();
        }
    }
}
