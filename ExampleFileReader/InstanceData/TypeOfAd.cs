using ExampleFileReader.InstanceData.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleFileReader.InstanceData
{
    public class TypeOfAd
    {
        private HashSet<Advertisement> ads = new HashSet<Advertisement>();

        public string ID { get; set; }

        public List<Advertisement> GetAds()
        {
            return ads.ToList();
        }

        public void AddAdvertisement(Advertisement advertisement)
        {
            ads.Add(advertisement);
            if (advertisement.Type != this)
            {
                advertisement.Type = this;
            }
        }
    }
}
