using ExampleFileReader.InstanceData.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleFileReader.InstanceData
{
    [Serializable]
    public class TvBreak
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan DesiredTimespan { get; set; }
        public int DesiredTimespanUnits { get; set; }
        public TimeSpan MaximumExtension { get; set; }
        public int MaximumExtensionUnits { get; set; }

        public List<AdvertisementInstance> Advertisements { get; set; } = new List<AdvertisementInstance>();

        public void AddAdvertisement(AdvertisementInstance ad)
        {
            Advertisements.Add(ad);
        }
    }
}
