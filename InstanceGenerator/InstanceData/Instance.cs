using InstanceGenerator.InstanceData.Activities;
using InstanceGenerator.Interfaces;
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
        [Description("Instance begining in ISO 8601 UTC format.")]
        public DateTime StartTime { get; set; }
        [Description("Instance end in ISO 8601 UTC format.")]
        public DateTime EndTime { get; set; }
        [Description("Instance span in '[d.]hh:mm:ss' format (the part in [] is optional).")]
        public TimeSpan Span { get; set; }
        [Description("Instance span in units.")]
        public int SpanUnits { get; set; }
        [Description("Chosen unit size in seconds.")]
        public double UnitSizeInSeconds { get; set; }
        
        public int ChannelAmountChecksum { get; set; }
        [JsonIgnore]
        public int ProgramAmountChecksum { get; set; }
        [JsonIgnore]
        public int AdsAmountChecksum { get; set; }

        [JsonProperty(Order = 1)]
        [Description("Dictionary declaring types of ads present in the instance.")]
        public Dictionary<int, TypeOfAd> TypesOfAds { get; set; } = new Dictionary<int, TypeOfAd>();

        [JsonProperty(Order = 2)]
        [Description("Dictionary declaring brands present in the instance.")]
        public Dictionary<int, Brand> Brands { get; set; } = new Dictionary<int, Brand>();

        [JsonProperty(Order = 3)]
        [Description("Brand compatibility matrix in sparse form (values not present are fully incompatible - hard constraint). Possible values: 0.0 - fully compatible, >0.0 - not preferred, acts as a loss function weight")]
        public Dictionary<int, Dictionary<int, double>> BrandIncompatibilityCost { get; set; } = new Dictionary<int, Dictionary<int, double>>();

        [JsonProperty(Order = 4)]
        [Description("Tasks - advertisements to schedule with their constraints.")]
        public Dictionary<int, AdvertisementOrder> AdOrders { get; set; } = new Dictionary<int, AdvertisementOrder>();

        [Description("Channels - 'machines' on which we schedule the tasks.")]
        [JsonProperty(Order = 5)]

        public Dictionary<string, Channel> Channels { get; set; } = new Dictionary<string, Channel>();

        [JsonProperty(Order = 6)]
        [Description("Type to break sparse compatibility matrix (values not present are compatible). Possible values: 1 - incompatible.")]
        public Dictionary<int, Dictionary<int, byte>> TypeToBreakIncompatibilityMatrix { get; set; } = new Dictionary<int, Dictionary<int, byte>>();




        public IEnumerable<Channel> GetChannelList()
        {
            return Channels.Values;
        }


        public IEnumerable<AdvertisementOrder> GetBlocksOfAdsList()
        {
            return AdOrders.Values;
        }

        public AdvertisementOrder GetOrAddOrderOfAds(int advertisementId)
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

        public TypeOfAd GetOrAddTypeOfAds(int blockId)
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

        public void AddBrandCompatibilityIfNotExists(int brand1, int brand2, double incompatibilityScore)
        {
            if (!BrandIncompatibilityCost.ContainsKey(brand1))
            {
                BrandIncompatibilityCost.Add(brand1, new Dictionary<int, double>());
            }
            if (!BrandIncompatibilityCost.ContainsKey(brand2))
            {
                BrandIncompatibilityCost.Add(brand2, new Dictionary<int, double>());
            }
            if (!BrandIncompatibilityCost[brand2].ContainsKey(brand1))
            {
                BrandIncompatibilityCost[brand2][brand1] = incompatibilityScore;
            }
            if (!BrandIncompatibilityCost[brand1].ContainsKey(brand2))
            {
                BrandIncompatibilityCost[brand1][brand2] = incompatibilityScore;
            }
        }

        public Brand GetOrAddBrand(int ownerId)
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
