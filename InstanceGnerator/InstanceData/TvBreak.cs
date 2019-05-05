using InstanceGenerator.InstanceData.Activities;
using InstanceGenerator.InstanceData.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGenerator.InstanceData
{
    [Serializable]
    public class TvBreak : IActivitiesSequence
    {
        private List<AdvertisementInstance> _advertisements = new List<AdvertisementInstance>();

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Span { get; set; }
        public int SpanUnits { get; set; }
        public TimeSpan MaximumExtension { get; set; }
        public int MaximumExtensionUnits { get; set; }
        public ViewershipFunction MainViewsFunction { get; set; }
        public Dictionary<string, ViewershipFunction> TypeViewsFunctions { get; set; } = new Dictionary<string, ViewershipFunction>();


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
