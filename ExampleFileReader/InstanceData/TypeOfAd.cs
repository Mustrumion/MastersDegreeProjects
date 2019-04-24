using ExampleFileReader.InstanceData.Activities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleFileReader.InstanceData
{
    public class TypeOfAd
    {
        [JsonIgnore]
        public List<Advertisement> Ads { get; set; } = new List<Advertisement>();

        public string ID { get; set; }

        public List<Advertisement> GetAds()
        {
            return Ads.ToList();
        }

        public void AddAdvertisement(Advertisement advertisement)
        {
            Ads.Add(advertisement);
            if (advertisement.Type != this)
            {
                advertisement.Type = this;
            }
        }
    }
}
