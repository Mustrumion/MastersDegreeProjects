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
    [Serializable]
    public class AdvertisementTask
    {
        /// <summary>
        /// Instances of advertisement from real data. Not used outside of instance creation.
        /// </summary>
        [JsonIgnore]
        public HashSet<AdvertisementInstance> AdvertisementInstances { get; set; } = new HashSet<AdvertisementInstance>();

        public int ID { get; set; }

        public int AdSpanUnits { get; set; }
        public TimeSpan AdSpan { get; set; }

        public double Gain { get; set; }
        public double MinViewership { get; set; }
        public int MinTimesAired { get; set; }
        public double MinBeginingsProportion { get; set; }
        public double MinEndsProportion { get; set; }
        public int MaxPerBlock { get; set; }
        public int MinJobsBetweenSame { get; set; }

        public DateTime DueTime { get; set; }
        public double OverdueCostParameter { get; set; }

        public TypeOfAd Type { get; set; }
        public Brand Brand { get; set; }
       
        public void AddAdvertisement(AdvertisementInstance advertisement)
        {
            AdvertisementInstances.Add(advertisement);
            if(advertisement.AdvertisementOrder != this)
            {
                advertisement.AdvertisementOrder = this;
            }
        }
    }
}
