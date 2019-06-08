﻿using InstanceGenerator.InstanceData;
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
        public Instance Instance { get; set; }
        public Solution Solution { get; set; }

        public AdvertisementOrder AdvertisementOrder { get; set; }
        public TvBreak TvBreak { get; set; }
        public TaskCompletionDifference OverallDifference { get; set; }

        public void Asses()
        {

        }

        public void Execute()
        {
            Solution.AddAdToBreak(AdvertisementOrder, TvBreak, 0);
        }

        public void RollBack()
        {
            Solution.RemoveAdFromBreak(AdvertisementOrder, TvBreak, 0);
        }
    }
}
