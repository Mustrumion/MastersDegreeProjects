using InstanceGenerator.InstanceData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGenerator.SolutionObjects
{
    public class BreakSchedule
    {
        public BreakSchedule(TvBreak tvBreak)
        {
            BreakData = tvBreak;
        }

        public TvBreak BreakData { get; set; }
        public List<AdvertisementOrder> Order { get; set; } = new List<AdvertisementOrder>();

        public int UnitFill => Order.Sum(a => a.AdSpanUnits);
    }
}
