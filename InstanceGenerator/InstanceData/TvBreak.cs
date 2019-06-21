using InstanceGenerator.InstanceData.Activities;
using InstanceGenerator.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGenerator.InstanceData
{
    /// <summary>
    /// Class containing instance data for the TV break.
    /// </summary>
    [Serializable]
    public class TvBreak : IActivitiesSequence
    {
        private List<AdvertisementInstance> _advertisements = new List<AdvertisementInstance>();

        public int ID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Span { get; set; }
        public int SpanUnits { get; set; }
        public ViewershipFunction MainViewsFunction { get; set; }
        public Dictionary<int, ViewershipFunction> TypeViewsFunctions { get; set; } = new Dictionary<int, ViewershipFunction>();

        public ViewershipFunction GetViewsFuntion(int typeID)
        {
            if(TypeViewsFunctions.TryGetValue(typeID, out var function))
            {
                return function;
            }
            return MainViewsFunction;
        }

        /// <summary>
        /// Advertisement instances from the real data. Used only for the instance generation.
        /// </summary>
        [JsonIgnore]
        public List<AdvertisementInstance> Advertisements
        {
            get => _advertisements;
            set
            {
                _advertisements = value;
                foreach(var ad in _advertisements)
                {
                    if (ad.Break != this)
                    {
                        ad.Break = this;
                    }
                }
            }
        }

        public void AddAdvertisement(AdvertisementInstance ad)
        {
            Advertisements.Add(ad);
            if (ad.Break != this)
            {
                ad.Break = this;
            }
        }

        [JsonIgnore]
        public List<BaseActivity> Activities { get; set; } = new List<BaseActivity>();
    }
}
