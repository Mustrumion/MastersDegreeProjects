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
    public class GreedyHeuristicSolver : ISolver
    {
        private Instance _instance;
        public Solution Solution { get; set; }
        public IScoringFunction ScoringFunction { get; set; }

        public Instance Instance
        {
            get
            {
                return _instance;
            }
            set
            {
                _instance = value;
                if(Solution == null)
                {
                    Solution = new Solution(value);
                }
            }
        }

        public string Description { get; set; }
        public int Seed
        {
            get => 0;
            set{ }
        }

        private bool _movePerformed;
        public void Solve()
        {
            ScoringFunction.AssesSolution(Solution);
            //due earlier are scheduled first
            //heftier are scheduled first if due at the same time
            while (Solution.CompletionScore <= 1 && _movePerformed)
            {
                foreach (AdvertisementOrder order in Instance.AdOrders.Values.OrderByDescending(order => order.AdSpanUnits).OrderBy(order => order.DueTime))
                {
                    TryToScheduleOrder(order);
                }
            }
        }

        private void TryToScheduleOrder(AdvertisementOrder order)
        {
            var orderData = Solution.AdOrderData[order.ID];
            if (orderData.Completed)
            {
                return;
            }
        }
    }
}
