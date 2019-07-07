using InstanceGenerator;
using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers
{
    public class RandomFastSolver : BaseSolver, ISolver
    {
        private List<AdvertisementTask> _order { get; set; }

        public RandomFastSolver() : base()
        {
        }

        protected override void InternalSolve()
        {
            _order = new List<AdvertisementTask>();
            foreach (var adInfo in Instance.AdOrders.Values)
            {
                for (int i = 0; i < adInfo.MinTimesAired; i++)
                {
                    _order.Add(adInfo);
                }
            }
            _order.Shuffle(Random);
            int curr = 0;
            var breakList = Solution.AdvertisementsScheduledOnBreaks.Values.ToList();
            breakList.Shuffle(Random);
            foreach (var schedule in breakList)
            {
                int currentSize = 0;
                while (curr < _order.Count)
                {
                    if ((currentSize += _order[curr].AdSpanUnits) >= schedule.BreakData.SpanUnits)
                    {
                        break;
                    }
                    schedule.AddAd(_order[curr]);
                    curr++;
                }
                schedule.Scores = null;
            }
            ScoringFunction.AssesSolution(Solution);
        }
    }
}
