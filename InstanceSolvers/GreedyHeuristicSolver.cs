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
    public class GreedyHeuristicSolver
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
                Solution = new Solution()
                {
                    Instance = Instance,
                };
            }
        }

        public void Solve()
        {
            //due earlier are scheduled first
            //heftier are scheduled first if due at the same time
            foreach(AdvertisementOrder order in Instance.AdOrders.Values.OrderByDescending(order => order.AdSpanUnits).OrderBy(order => order.DueTime))
            {
                ScheduleOrder(order);
            }
        }

        private void ScheduleOrder(AdvertisementOrder order)
        {
            throw new NotImplementedException();
        }
    }
}
