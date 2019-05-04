using ExampleFileReader.InstanceData.Activities;
using ExampleFileReader.InstanceData.Interfaces;
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

        public int UnitSpan { get; set; }
        public TimeSpan AdSpan { get; set; }

        public double Gain { get; set; }
        public double MinViewership { get; set; }
        public int MinTimesAired { get; set; }
        public double MinBeginingsProportion { get; set; }
        public double MinEndsProportion { get; set; }
        public int MaxPerBlock { get; set; }
        public int MinJobsBetweenSame { get; set; }

        public DateTime DueTime { get; set; }
        public double LossPerDay { get; set; }

        public TypeOfAd Type { get; set; }
        public OwnerOfAd Owner { get; set; }
       
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
