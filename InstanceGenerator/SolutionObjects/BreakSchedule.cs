﻿using InstanceGenerator.InstanceData;
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

        public BreakSchedule(TvBreak tvBreak, List<AdvertisementOrder> order) : this(tvBreak)
        {
            SubsituteOrder(order);
        }

        public TvBreak BreakData { get; set; }
        private List<AdvertisementOrder> _order = new List<AdvertisementOrder>();
        private int _unitFill;

        public IReadOnlyList<AdvertisementOrder> Order => _order.AsReadOnly();

        public List<AdvertisementOrder> GetOrderCopy()
        {
            return _order.ToList();
        }

        public List<int> GetOrderIdsCopy()
        {
            return _order.Select(a => a.ID).ToList();
        }

        public void AddAd(AdvertisementOrder ad)
        {
            _order.Add(ad);
            _unitFill += ad.AdSpanUnits;
        }

        public void AddAdRange(IEnumerable<AdvertisementOrder> ads)
        {
            _order.AddRange(ads);
            _unitFill += ads.Sum(a => a.AdSpanUnits);
        }

        public void SubsituteOrder(List<AdvertisementOrder> ads)
        {
            _order = ads;
            _unitFill = ads.Sum(a => a.AdSpanUnits);
        }

        public void RemoveAt(int position)
        {
            _unitFill -= _order[position].AdSpanUnits;
            _order.RemoveAt(position);
        }

        public void Insert(int position, AdvertisementOrder ad)
        {
            _order.Insert(position, ad);
            _unitFill += ad.AdSpanUnits;
        }

        public int Count => _order.Count;

        public int UnitFill => _unitFill;
    }
}