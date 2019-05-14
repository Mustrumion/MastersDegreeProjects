using InstanceGenerator.InstanceData.Activities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGenerator.InstanceData
{
    public class TypeOfAd
    {
        /// <summary>
        /// Advertisement instances in the real data. Used only for the instance generation.
        /// </summary>
        [JsonIgnore]
        public HashSet<AdvertisementInstance> Ads { get; set; } = new HashSet<AdvertisementInstance>();

        public int ID { get; set; }

        public List<AdvertisementInstance> GetAds()
        {
            return Ads.ToList();
        }

        public void AddAdvertisement(AdvertisementInstance advertisement)
        {
            Ads.Add(advertisement);
            if (advertisement.Type != this)
            {
                advertisement.Type = this;
            }
        }


        public void JoinType(TypeOfAd type)
        {
            foreach(var advertisement in type.Ads)
            {
                this.AddAdvertisement(advertisement);
            }
        }
    }
}
