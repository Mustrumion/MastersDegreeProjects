using InstanceGenerator.InstanceData.Activities;
using InstanceGenerator.InstanceData.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGenerator.InstanceData
{
    public class Instance: ISpannedObject
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Span { get; set; }
        public int SpanUnits { get; set; }
        public double UnitSizeInSeconds { get; set; }
        
        public int ChannelAmountChecksum { get; set; }
        [JsonIgnore]
        public int ProgramAmountChecksum { get; set; }
        [JsonIgnore]
        public int AdsAmountChecksum { get; set; }

        [JsonProperty(Order = 1)]
        [Description("Dictionary declaring types of ads present in the instance.")]
        public Dictionary<string, TypeOfAd> TypesOfAds { get; set; } = new Dictionary<string, TypeOfAd>();
        [JsonProperty(Order = 2)]
        [Description("Dictionary declaring brands present in the instance.")]
        public Dictionary<string, Brand> Brands { get; set; } = new Dictionary<string, Brand>();
        [JsonProperty(Order = 3)]
        [Description("Brand compatibility matrix in sparse form (value not present means full incompatibility). Values 0 - fully compatible, >0 - not preferred, acts as cost")]
        public Dictionary<string, Dictionary<string, double>> BrandConflictMatrix { get; set; } = new Dictionary<string, Dictionary<string, double>>();
        [JsonProperty(Order = 4)]
        [Description("Tasks - advertisements to schedule with their constraints.")]
        public Dictionary<string, AdvertisementOrder> AdOrders { get; set; } = new Dictionary<string, AdvertisementOrder>();
        [Description("Channels - 'machines' on which we schedule the tasks.")]
        [JsonProperty(Order = 5)]
        public Dictionary<string, Channel> Channels { get; set; } = new Dictionary<string, Channel>();
        public IEnumerable<Channel> GetChannelList()
        {
            return Channels.Values;
        }


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

        public IEnumerable<Brand> GetBrandsList()
        {
            return Brands.Values;
        }

        public void AddBrandCompatibilityIfNotExists(string brand1, string brand2, double incompatibilityScore)
        {
            if (!BrandConflictMatrix.ContainsKey(brand1))
            {
                BrandConflictMatrix.Add(brand1, new Dictionary<string, double>());
            }
            if (!BrandConflictMatrix.ContainsKey(brand2))
            {
                BrandConflictMatrix.Add(brand2, new Dictionary<string, double>());
            }
            if (!BrandConflictMatrix[brand2].ContainsKey(brand1))
            {
                BrandConflictMatrix[brand2][brand1] = incompatibilityScore;
            }
            if (!BrandConflictMatrix[brand1].ContainsKey(brand2))
            {
                BrandConflictMatrix[brand1][brand2] = incompatibilityScore;
            }
        }

        public Brand GetOrAddBrand(string ownerId)
        {
            if (Brands.ContainsKey(ownerId))
            {
                return Brands[ownerId];
            }
            Brand owner = new Brand()
            {
                ID = ownerId,
            };
            Brands[ownerId] = owner;
            return owner;
        }
    }
}
