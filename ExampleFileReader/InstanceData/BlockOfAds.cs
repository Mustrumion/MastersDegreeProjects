using ExampleFileReader.InstanceData.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleFileReader.InstanceData
{
    [Serializable]
    public class BlockOfAds
    {
        public List<Advertisement> Advertisements { get; set; } = new List<Advertisement>();

        public string ID { get; set; }
       

        public void AddAdvertisement(Advertisement advertisement)
        {
            Advertisements.Add(advertisement);
            if(advertisement.Block != this)
            {
                advertisement.Block = this;
            }
        }
    }
}
