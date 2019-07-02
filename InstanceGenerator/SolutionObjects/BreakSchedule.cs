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
        public Dictionary<int, TaskData> Scores { get; set; }


        private List<AdvertisementTask> _order = new List<AdvertisementTask>();
        private int _unitFill;

        public IReadOnlyList<AdvertisementTask> Order => _order.AsReadOnly();

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
            _unitFill += ad.AdSpanUnits;
        }

        public void AddAdRange(IEnumerable<AdvertisementTask> ads)
        {
            _order.AddRange(ads);
            _unitFill += ads.Sum(a => a.AdSpanUnits);
        }

        public void SubsituteOrder(List<AdvertisementTask> ads)
        {
            _order = ads;
            _unitFill = ads.Sum(a => a.AdSpanUnits);
        }

        public void RemoveAt(int position)
        {
            _unitFill -= _order[position].AdSpanUnits;
            _order.RemoveAt(position);
        }

        public void Insert(int position, AdvertisementTask ad)
        {
            _order.Insert(position, ad);
            _unitFill += ad.AdSpanUnits;
        }

        public void Append(AdvertisementTask ad)
        {
            _order.Add(ad);
            _unitFill += ad.AdSpanUnits;
        }

        public int Count => _order.Count;

        public int UnitFill => _unitFill;
    }
}
