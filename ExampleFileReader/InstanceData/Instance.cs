using ExampleFileReader.InstanceData.Activities;
using ExampleFileReader.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleFileReader.InstanceData
{
    public class Instance
    {
        [JsonProperty(Order = 3)]
        public Dictionary<string, Channel> Channels { get; set; } = new SerializableDictionary<string, Channel>();
        public IEnumerable<Channel> GetChannelList()
        {
            return Channels.Values;
        }

        [JsonProperty(Order = 2)]
        public Dictionary<string, AdvertisementOrder> AdOrders { get; set; } = new SerializableDictionary<string, AdvertisementOrder>();
        public IEnumerable<AdvertisementOrder> GetBlocksOfAdsList()
        {
            return AdOrders.Values;
        }

        public AdvertisementOrder GetOrAddOrderOfAds(string advertisementId)
        {
            if (AdOrders.ContainsKey(advertisementId))
            {
                return AdOrders[advertisementId];
            }
            AdvertisementOrder advert = new AdvertisementOrder()
            {
                ID = advertisementId,
            };
            AdOrders[advertisementId] = advert;
            return advert;
        }

        [JsonProperty(Order = 1)]
        public Dictionary<string, TypeOfAd> TypesOfAds { get; set; } = new SerializableDictionary<string, TypeOfAd>();

        public IEnumerable<TypeOfAd> GetTypesOfAdsList()
        {
            return TypesOfAds.Values;
        }

        public TypeOfAd GetOrAddTypeOfAds(string blockId)
        {
            if (TypesOfAds.ContainsKey(blockId))
            {
                return TypesOfAds[blockId];
            }
            TypeOfAd block = new TypeOfAd()
            {
                ID = blockId,
            };
            TypesOfAds[blockId] = block;
            return block;
        }



        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public int ChannelAmountChecksum { get; set; }
        public int ProgramAmountChecksum { get; set; }
        public int AdsAmountChecksum { get; set; }
    }
}
