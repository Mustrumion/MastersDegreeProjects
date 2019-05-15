using InstanceGenerator.InstanceData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGenerator.SolutionObjects
{
    public class AdOrder
    {
        public int ID { get; set; }

        public double OverdueLoss { get; set; }
        public double OwnerSoftConflictsLoss { get; set; }
        public double OutOfBreakLoss { get; set; }

        public double Wievership { get; set; }

        public int NumberOfStarts { get; set; }
        public int NumberOfEnds { get; set; }
        public int OwnerConflicts { get; set; }
        public int BreakTypeConflicts { get; set; }
        public int SelfSpacingConflicts { get; set; }
        public int SelfIncompatibilityConflicts { get; set; }

        public AdvertisementOrder AdvertisementOrderConstraints { get; set; }

        public Dictionary<int, List<int>> BreaksPositions { get; set; } = new Dictionary<int, List<int>>();
    }
}
