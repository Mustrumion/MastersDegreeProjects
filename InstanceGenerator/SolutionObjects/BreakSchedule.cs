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

        public BreakSchedule(TvBreak tvBreak, List<AdvertisementTask> order) : this(tvBreak)
        {
            SubsituteOrder(order);
        }

        public TvBreak BreakData { get; set; }
        public Dictionary<int, TaskScore> Scores { get; set; }


        private List<AdvertisementTask> _order = new List<AdvertisementTask>();
        private List<int> _endTimes = new List<int>();

        public IReadOnlyList<AdvertisementTask> Order => _order.AsReadOnly();
        public IReadOnlyList<int> EndTimes => _endTimes.AsReadOnly();

        public List<AdvertisementTask> GetOrderCopy()
        {
            return _order.ToList();
        }

        public BreakSchedule DeepClone()
        {
            return new BreakSchedule(BreakData, Order.ToList())
            {
                Scores = Scores,
            };
        }

        public List<int> GetOrderIdsCopy()
        {
            return _order.Select(a => a.ID).ToList();
        }

        public void AddAd(AdvertisementTask ad)
        {
            _order.Add(ad);
            _endTimes.Add(_endTimes.LastOrDefault() + ad.AdSpanUnits);
        }

        public void AddAdRange(IEnumerable<AdvertisementTask> ads)
        {
            _order.AddRange(ads);
            foreach(var ad in ads)
            {
                _endTimes.Add(_endTimes.LastOrDefault() + ad.AdSpanUnits);
            }
        }

        public void SubsituteOrder(List<AdvertisementTask> ads)
        {
            _order = ads;
            foreach(var ad in ads)
            {
                _endTimes.Add(_endTimes.LastOrDefault() + ad.AdSpanUnits);
            }
        }

        public void RemoveAt(int position)
        {
            var ad = _order[position];
            _order.RemoveAt(position);
            _endTimes.RemoveAt(position);
            for(int i = position; i < _endTimes.Count; i++)
            {
                _endTimes[i] -= ad.AdSpanUnits;
            }
        }

        public void Insert(int position, AdvertisementTask ad)
        {
            _order.Insert(position, ad);
            int previousEnd = position - 1 >= 0 ? _endTimes[position - 1] : 0;
            _endTimes.Insert(position, previousEnd + ad.AdSpanUnits);
            for (int i = position + 1; i < _endTimes.Count; i++)
            {
                _endTimes[i] += ad.AdSpanUnits;
            }
        }

        public int Count => _order.Count;

        public int UnitFill => _endTimes.LastOrDefault();

        public int ID => BreakData.ID;
    }
}
