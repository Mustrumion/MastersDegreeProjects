using ExampleFileReader.InstanceData.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleFileReader.InstanceData
{
    [Serializable]
    public class AdvertisementOrder
    {
        public List<AdvertisementInstance> Advertisements { get; set; } = new List<AdvertisementInstance>();

        public string ID { get; set; }
       

        public void AddAdvertisement(AdvertisementInstance advertisement)
        {
            Advertisements.Add(advertisement);
            if(advertisement.AdvertisementOrder != this)
            {
                advertisement.AdvertisementOrder = this;
            }
        }
    }
}
