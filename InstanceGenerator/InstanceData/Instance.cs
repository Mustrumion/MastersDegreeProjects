using InstanceGenerator.InstanceData.Activities;
using InstanceGenerator.Interfaces;
using InstanceGenerator.SolutionObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGenerator.InstanceData
{
    public class Instance : ISpannedObject
    {
        public string Description { get; set; }

        private Dictionary<string, Channel> _channels = new Dictionary<string, Channel>();

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

        public int NumberOfCampaigns { get => AdOrders.Count; }
        public int NumberOfChannels { get => Channels.Count; }
        public int NumberOfAdsToSchedule { get => AdOrders.Values.Sum(t => t.MinTimesAired); }
        public int NumberOfBreaks { get => Breaks.Count; }
        public double AverageBreakLengthMinutes { get => Breaks.Values.Average(b => b.Span.TotalMinutes); }
        public string HorizonType { get => Span.TotalDays < 10.0 ? "week" : "month"; }

        /// <summary>
        /// Dictionary declaring types of ads present in the instance.
        /// </summary>
        [JsonProperty(Order = 1)]
        [Description("Dictionary declaring types of ads present in the instance.")]
        public Dictionary<int, TypeOfAd> TypesOfAds { get; set; } = new Dictionary<int, TypeOfAd>();

        /// <summary>
        /// Dictionary declaring brands present in the instance.
        /// </summary>
        [JsonProperty(Order = 2)]
        [Description("Dictionary declaring brands present in the instance.")]
        public Dictionary<int, Brand> Brands { get; set; } = new Dictionary<int, Brand>();

        /// <summary>
        /// Tasks - advertisements to schedule with their constraints
        /// </summary>
        [JsonProperty(Order = 3)]
        [Description("Tasks - advertisements to schedule with their constraints.")]
        public Dictionary<int, AdvertisementTask> AdOrders { get; set; } = new Dictionary<int, AdvertisementTask>();

        /// <summary>
        /// Channels - 'machines' on which we schedule the tasks.
        /// </summary>
        [Description("Channels - 'machines' on which we schedule the tasks.")]
        [JsonProperty(Order = 4)]
        public Dictionary<string, Channel> Channels
        {
            get => _channels;
            set
            {
                _channels = value;
                RestoreBreakDictionary();
            }
        }


        /// <summary>
        /// Brand compatibility matrix in sparse form (values not present are fully incompatible - hard constraint). Possible values: 0.0 - fully compatible, >0.0 - not preferred, acts as a loss function weight
        /// </summary>
        [JsonProperty(Order = 5)]
        [Description("Brand compatibility matrix in sparse form (values not present are fully incompatible - hard constraint). Possible values: 0.0 - fully compatible, >0.0 - not preferred, acts as a loss function weight")]
        public Dictionary<int, Dictionary<int, double>> BrandIncompatibilityCost { get; set; } = new Dictionary<int, Dictionary<int, double>>();
        

        /// <summary>
        /// Type to break sparse compatibility matrix (values not present are compatible). Possible values: 1 - incompatible.
        /// </summary>
        [JsonProperty(Order = 6)]
        [Description("Type to break sparse compatibility matrix (values not present are compatible). Possible values: 1 - incompatible.")]
        public Dictionary<int, Dictionary<int, byte>> TypeToBreakIncompatibilityMatrix { get; set; } = new Dictionary<int, Dictionary<int, byte>>();

        /// <summary>
        /// All breaks in instance mapped by their ID
        /// </summary>
        [JsonIgnore]
        public Dictionary<int, TvBreak> Breaks { get; set; } = new Dictionary<int, TvBreak>();

        public IEnumerable<Channel> GetChannelList()
        {
            return Channels.Values;
        }


        public IEnumerable<AdvertisementTask> GetBlocksOfAdsList()
        {
            return AdOrders.Values;
        }

        public AdvertisementTask GetOrAddOrderOfAds(int advertisementId)
        {
            if (AdOrders.ContainsKey(advertisementId))
            {
                return AdOrders[advertisementId];
            }
            AdvertisementTask advert = new AdvertisementTask()
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

        public TypeOfAd GetOrAddTypeOfAds(int typeId)
        {
            if (TypesOfAds.ContainsKey(typeId))
            {
                return TypesOfAds[typeId];
            }
            TypeOfAd type = new TypeOfAd()
            {
                ID = typeId,
            };
            TypesOfAds[typeId] = type;
            return type;
        }

        public IEnumerable<Brand> GetBrandsList()
        {
            return Brands.Values;
        }

        public void AddBrandCompatibility(int brand1, int brand2, double incompatibilityScore)
        {
            if (!BrandIncompatibilityCost.TryGetValue(brand1, out var brand1Dict))
            {
                brand1Dict = new Dictionary<int, double>();
                BrandIncompatibilityCost.Add(brand1, brand1Dict);
            }
            if (!BrandIncompatibilityCost.TryGetValue(brand2, out var brand2Dict))
            {
                brand2Dict = new Dictionary<int, double>();
                BrandIncompatibilityCost.Add(brand2, brand2Dict);
            }
            brand2Dict[brand1] = incompatibilityScore;
            brand1Dict[brand2] = incompatibilityScore;
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

        public void RestoreBreakDictionary()
        {
            foreach (var channel in _channels)
            {
                foreach (var tvBreak in channel.Value.Breaks)
                {
                    Breaks[tvBreak.ID] = tvBreak;
                }
            }
        }

        public void RestoreStructuresAfterDeserialization()
        {
            RestoreBreakDictionary();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ad"></param>
        /// <param name="schedule"></param>
        /// <returns>0 - compatible, 1 - incompatible</returns>
        public byte GetTypeToBreakIncompatibility(TaskScore ad, BreakSchedule schedule)
        {
            return GetTypeToBreakIncompatibility(ad.AdConstraints.Type.ID, schedule.BreakData.ID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeId"></param>
        /// <param name="breakId"></param>
        /// <returns>0 - compatible, 1 - incompatible</returns>
        public byte GetTypeToBreakIncompatibility(int typeId, int breakId)
        {
            if (TypeToBreakIncompatibilityMatrix.TryGetValue(typeId, out var incompatibleBreaks))
            {
                if (incompatibleBreaks.ContainsKey(breakId))
                {
                    return 1;
                }
            }
            return 0;
        }


        public double GetBrandsIncompatibility(int brand1Id, int brand2Id)
        {
            if (brand1Id == brand2Id)
            {
                return 0;
            }
            if (!BrandIncompatibilityCost.TryGetValue(brand1Id, out var brandCompatibility))
            {
                return double.PositiveInfinity;
            }
            if (!brandCompatibility.TryGetValue(brand2Id, out var incompatibilityScore))
            {
                return double.PositiveInfinity;
            }
            return incompatibilityScore;
        }

        public double GetAdsBrandIncompatibilitiy(AdvertisementTask ad1, AdvertisementTask ad2)
        {
            if (ad1.Type.ID != ad2.Type.ID)
            {
                return 0;
            }
            return GetBrandsIncompatibility(ad1.Brand.ID, ad2.Brand.ID);
        }

        public List<double> GetBulkBrandIncompatibilities(AdvertisementTask ad1, IEnumerable<AdvertisementTask> otherAds)
        {
            BrandIncompatibilityCost.TryGetValue(ad1.Brand.ID, out var ad1BrandDict);
            return otherAds.Select(ad2 =>
            {
                if (ad1.Brand.ID == ad2.Brand.ID) return 0;
                if (ad1BrandDict == null) return double.PositiveInfinity;
                if (!ad1BrandDict.TryGetValue(ad2.Brand.ID, out var incompatibilityScore))
                {
                    return double.PositiveInfinity;
                }
                return incompatibilityScore;
            }).ToList();
        }
    }
}
