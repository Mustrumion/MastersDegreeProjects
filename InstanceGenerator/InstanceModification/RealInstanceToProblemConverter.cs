using InstanceGenerator.InstanceData;
using InstanceGenerator.InstanceData.Activities;
using InstanceGenerator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGenerator.InstanceModification
{
    public class RealInstanceToProblemConverter
    {
        public Instance Instance { get; set; }
        public double TimeUnitInSeconds { get; set; } = 3.0d;
        public double MinBeginingsProportionMultiplier { get; set; } = 0.8d;
        public int MinBeginingsProportionOffset { get; set; } = -1;
        public double MinEndsProportionMultiplier { get; set; } = 0.8d;
        public int MinEndsProportionOffset { get; set; } = -1;
        public double MinViewsMultiplier { get; set; } = 0.999d;
        public int MaxAdsPerBreakOffset { get; set; } = 1;
        public int DefaultAdsInBetweenSame { get; set; } = 99999;
        public int MinAdsInBetweenSameOffset { get; set; } = -1;
        public int MinTimesAiredOffset { get; set; } = 0;
        public TimeSpan DueTimeOffset { get; set; } = new TimeSpan(0, 0, 0, 0);
        public string InstanceDescription { get; set; }
        public bool RoundDueTimeUpToDays { get; set; } = false;
        public bool MinTimesAiredWeighted { get; set; } = false; //changing to true will often result in solution based on real data being not acceptable
        public bool MakeEverythingCompatible { get; set; } = false;

        private Dictionary<int, int> nightTypesCount;
        private Dictionary<int, int> dayTypesCount;
        private HashSet<TvBreak> nightlyBreaks;
        private HashSet<TvBreak> dailyBreaks;
        private TimeSpan nightsEnd = new TimeSpan(6, 0, 0);
        private TimeSpan nightsStart = new TimeSpan(22, 0, 0);

        int lastBreakId;


        public void ConvertToProblem()
        {
            CombineAdTypes();
            ConvertToUnits();
            InstanceCorrector corrector = new InstanceCorrector() { Instance = Instance };
            corrector.AdjustTimestampsToDuration();
            CreateBreaks();
            Instance.RestoreBreakDictionary();
            AddViewershipFunctions();
            RemoveUnnecessaryActivities();
            GenerateAllAdOrdersData();
            corrector.AdjustAdLengthToChosenOrderLength();
            corrector.AdjustAdViewsToCurrentPositionViews();
            corrector.AdjustTimestampsToDuration();
            RecalculateAdOrderViewership();
            GenerateBreakToTypeCompatibilityMatrix();
            AssureBrandToBrandCompatibility();
            Instance.Description = InstanceDescription;
        }


        public void AssureBrandToBrandCompatibility()
        {
            foreach(var tvBreak in Instance.Breaks.Values)
            {
                int startingPos = 0;
                for(int i = startingPos; i < tvBreak.Advertisements.Count - 1; i++)
                {
                    Instance.AddBrandCompatibility(tvBreak.Advertisements[i].OwnerID, tvBreak.Advertisements[i + 1].OwnerID, 0);
                    for(int j = i + 2; j < tvBreak.Advertisements.Count; j++)
                    {
                        Instance.AddBrandCompatibility(tvBreak.Advertisements[i].OwnerID, tvBreak.Advertisements[j].OwnerID, 1.0);
                    }
                }
                startingPos += 1;
            }
        }

        public void CreateBreaks()
        {
            lastBreakId = 0;
            foreach (Channel channel in Instance.GetChannelList())
            {
                CreateChannelBreaks(channel);
            }
        }

        private void CreateChannelBreaks(Channel channel)
        {
            TvBreak currentBreak = null;
            foreach (BaseActivity activity in channel.Activities)
            {
                if (activity is TvProgram)
                {
                    TvProgram program = activity as TvProgram;
                    AddBreakToChannelIfNotEmpty(currentBreak, channel);
                    currentBreak = new TvBreak()
                    {
                        StartTime = program.EndTime,
                    };
                }
                else
                {
                    if (currentBreak == null)
                    {
                        currentBreak = new TvBreak()
                        {
                            StartTime = activity.StartTime,
                        };
                    }
                    currentBreak.Activities.Add(activity);
                    if(activity is AdvertisementInstance)
                    {
                        currentBreak.AddAdvertisement(activity as AdvertisementInstance);
                    }
                }
            }
            AddBreakToChannelIfNotEmpty(currentBreak, channel);
        }


        private void AddBreakToChannelIfNotEmpty(TvBreak tvBreak, Channel channel)
        {
            if (tvBreak != null && tvBreak.Advertisements.Count > 0)
            {
                tvBreak.ID = lastBreakId++;
                tvBreak.EndTime = tvBreak.Advertisements.Last().EndTime;
                RecalculateSpanToUnits(tvBreak);
                channel.AddBreak(tvBreak);
            }
        }



        public void AddViewershipFunctions()
        {
            foreach(var channel in Instance.Channels.Values)
            {
                foreach(var tvBreak in channel.Breaks)
                {
                    AddBreakViewershipFunction(tvBreak);
                }
            }
        }

        private void AddBreakViewershipFunction(TvBreak tvBreak)
        {
            ViewershipFunction function = new ViewershipFunction();
            AdvertisementInstance lastAd = new AdvertisementInstance();
            foreach(BaseActivity activity in tvBreak.Activities)
            {
                ViewershipSpan currentSpan = null;
                if (activity is Autopromotion autopromotion)
                {
                    currentSpan = new ViewershipSpan(autopromotion);
                }
                if (activity is AdvertisementInstance advertisement)
                {
                    currentSpan = new ViewershipSpan(advertisement);
                }
                function.AddTimeInterval(currentSpan);
            }
            tvBreak.MainViewsFunction = function;
        }

        public void RemoveUnnecessaryActivities()
        {
            foreach(var channel in Instance.Channels.Values)
            {
                channel.Autopromotions.Clear();
                channel.Programs.Clear();
                channel.Activities.RemoveAll(a => a is TvProgram || a is Autopromotion);
                foreach(var tvBreak in channel.Breaks)
                {
                    tvBreak.Activities.RemoveAll(a => a is Autopromotion);
                }
            }
        }
        
        public void ConvertToUnits()
        {
            Instance.UnitSizeInSeconds = TimeUnitInSeconds;
            RecalculateSpanToUnits(Instance);
            foreach (var channel in Instance.Channels.Values)
            {
                RecalculateSpanToUnits(channel);
                ConvertBreaksToUnits(channel);
                ConvertActivitiesToUnits(channel);
            }
        }

        private void RecalculateSpanToUnits(ISpannedObject spannedObj)
        {
            TimeSpan span = spannedObj.EndTime - spannedObj.StartTime;
            double spanSeconds = Helpers.NearestDivisibleBy(span.TotalSeconds, TimeUnitInSeconds, out int number);
            spannedObj.Span = new TimeSpan(0, 0, Convert.ToInt32(spanSeconds));
            spannedObj.SpanUnits = number;
            spannedObj.EndTime = spannedObj.StartTime + spannedObj.Span;
        }

        private void ConvertBreaksToUnits(Channel channel)
        {
            foreach (var adBreak in channel.Breaks)
            {
                RecalculateSpanToUnits(adBreak);
            }
        }

        private void ConvertActivitiesToUnits(Channel channel)
        {
            foreach (var activity in channel.Activities)
            {
                RecalculateSpanToUnits(activity);
            }
        }


        public void GenerateAllAdOrdersData()
        {
            foreach(AdvertisementTask order in Instance.AdOrders.Values)
            {
                GenerateAdOrderData(order);
            }
        }

        private void CountMinTimesAired(AdvertisementTask order)
        {
            TimeSpan sumSpan = new TimeSpan(0, 0, 0);
            foreach(AdvertisementInstance ad in order.AdvertisementInstances)
            {
                sumSpan += ad.Span;
            }
            int requiredAmount = order.AdvertisementInstances.Count;
            if (MinTimesAiredWeighted)
            {
                requiredAmount = (int)(sumSpan.TotalMilliseconds / order.AdSpan.TotalMilliseconds);
            }
            order.MinTimesAired = Math.Max(requiredAmount + MinTimesAiredOffset, 0);
        }

        private void GenerateSelfIncompatibilityData(AdvertisementTask order)
        {
            int minSelfInterval = DefaultAdsInBetweenSame;
            int maxAiredInBlock = 0;
            foreach(var ad in order.AdvertisementInstances)
            {
                var adsInThisBreak = ad.Break.Advertisements;
                int indexStarting = adsInThisBreak.IndexOf(ad);
                int times = 0;
                for(int i = indexStarting + 1; i < adsInThisBreak.Count; i++)
                {
                    if(adsInThisBreak[i].AdvertisementOrder == order)
                    {
                        times += 1;
                        if (minSelfInterval > i - indexStarting - 1)
                        {
                            minSelfInterval = i - indexStarting - 1;
                        }
                    }
                }
                if(maxAiredInBlock < times)
                {
                    maxAiredInBlock = times;
                }
            }
            order.MinJobsBetweenSame = Math.Max(minSelfInterval + MinAdsInBetweenSameOffset, 0);
            order.MaxPerBlock = Math.Max(maxAiredInBlock + MaxAdsPerBreakOffset, 1);
        }


        public void GenerateBreakToTypeCompatibilityMatrix()
        {
            nightTypesCount = new Dictionary<int, int>();
            dayTypesCount = new Dictionary<int, int>();
            nightlyBreaks = new HashSet<TvBreak>();
            dailyBreaks = new HashSet<TvBreak>();
            foreach (var channel in Instance.Channels.Values)
            {
                foreach(var tvBreak in channel.Breaks)
                {
                    AssignBreakToDayPeriod(tvBreak);
                    foreach(var ad in tvBreak.Advertisements)
                    {
                        AddAdvertisementToDayPeriodCounts(ad);
                    }
                }
            }
            GenerateMatrixFromData();
        }

        private void GenerateMatrixFromData()
        {
            double cutoffTreshold = 1d;
            foreach(var type in Instance.TypesOfAds.Values)
            {
                if (dayTypesCount.ContainsKey(type.ID) && nightlyBreaks.Count > 0)
                {
                    if(dayTypesCount[type.ID]/type.Ads.Count >= cutoffTreshold)
                    {
                        AddIncompatibility(type, nightlyBreaks);
                    }
                }
                if (nightTypesCount.ContainsKey(type.ID) && dailyBreaks.Count > 0)
                {
                    if (nightTypesCount[type.ID] / type.Ads.Count >= cutoffTreshold)
                    {
                        AddIncompatibility(type, dailyBreaks);
                    }
                }
            }
        }

        private void AddIncompatibility(TypeOfAd type, HashSet<TvBreak> breaks)
        {
            Instance.TypeToBreakIncompatibilityMatrix[type.ID] = new Dictionary<int, byte>();
            foreach(TvBreak tvBreak in breaks)
            {
                Instance.TypeToBreakIncompatibilityMatrix[type.ID][tvBreak.ID] = 1;
            }
        }

        private void AddAdvertisementToDayPeriodCounts(AdvertisementInstance ad)
        {
            if (ad.StartTime.TimeOfDay <= nightsEnd && ad.EndTime.TimeOfDay <= nightsEnd || ad.StartTime.TimeOfDay >= nightsStart && ad.EndTime.TimeOfDay >= nightsStart)
            {
                if (!nightTypesCount.ContainsKey(ad.TypeID))
                {
                    nightTypesCount[ad.TypeID] = 1;
                }
                else
                {
                    nightTypesCount[ad.TypeID] += 1;
                }
            }
            else
            {
                if (!dayTypesCount.ContainsKey(ad.TypeID))
                {
                    dayTypesCount[ad.TypeID] = 1;
                }
                else
                {
                    dayTypesCount[ad.TypeID] += 1;
                }
            }
        }

        private void AssignBreakToDayPeriod(TvBreak tvBreak)
        {
            if (tvBreak.StartTime.TimeOfDay <= nightsEnd || tvBreak.StartTime.TimeOfDay >= nightsStart)
            {
                nightlyBreaks.Add(tvBreak);
            }
            else
            {
                dailyBreaks.Add(tvBreak);
            }
        }

        private void RecalculateAdOrderViewership()
        {
            foreach(var order in Instance.AdOrders.Values)
            {
                order.MinViewership = order.AdvertisementInstances.Sum(a => a.Viewers) * MinViewsMultiplier;
            }
        }

        private void GenerateAdOrderData(AdvertisementTask order)
        {
            order.Gain = order.AdvertisementInstances.Sum(a => a.Profit);
            order.DueTime = order.AdvertisementInstances.OrderBy(a => a.EndTime).Last().EndTime + DueTimeOffset;
            if (RoundDueTimeUpToDays)
            {
                order.DueTime = order.DueTime.AddDays(1).Date;
            }
            order.MinViewership = order.AdvertisementInstances.Sum(a => a.Viewers) * MinViewsMultiplier;
            //Ads with same advertisement ID had different spans in real data, we choose the most frequent one here
            var modeSpanGroup = order.AdvertisementInstances.ToLookup(a => a.SpanUnits).OrderBy(cat => cat.Count()).Last();
            order.AdSpan = modeSpanGroup.First().Span;
            order.AdSpanUnits = modeSpanGroup.First().SpanUnits;
            //As a consequence we count the times aired requirements based on total span and above chosen single ad span
            CountMinTimesAired(order);
            int minBeginingsCount = order.AdvertisementInstances.Where(a => a.Break.Advertisements.First() == a).Count();
            order.MinBeginingsProportion = Math.Max(0, minBeginingsCount + MinBeginingsProportionOffset);
            order.MinBeginingsProportion /= order.AdvertisementInstances.Count();
            order.MinBeginingsProportion *= MinBeginingsProportionMultiplier;
            int minEndsCount = order.AdvertisementInstances.Where(a => a.Break.Advertisements.Last() == a).Count();
            order.MinEndsProportion = Math.Max(0, minEndsCount + MinEndsProportionOffset);
            order.MinEndsProportion /= order.AdvertisementInstances.Count();
            order.MinEndsProportion *= MinEndsProportionMultiplier;
            order.Type = order.AdvertisementInstances.First().Type;
            order.Brand = order.AdvertisementInstances.First().Brand;
            GenerateSelfIncompatibilityData(order);
            order.OverdueCostParameter = order.Gain;
        }

        /// <summary>
        /// Joins advertisement types on second category level.
        /// Advertisement types have 6 digit ID number
        /// Each two didigts represent a level in category tree 
        /// This function basically lumps all types that have same four digits.
        /// </summary>
        public void CombineAdTypes()
        {
            var typesGroups = Instance.TypesOfAds.Values.ToLookup(t => t.ID.ToString().PadLeft(6, '0').Substring(0, 4)).ToList();
            foreach(var group in typesGroups)
            {
                CombineAdGroup(group);
            }
        }

        private void CombineAdGroup(IGrouping<string,  TypeOfAd> group)
        {
            TypeOfAd firstType = group.First();
            Instance.TypesOfAds.Remove(firstType.ID);
            firstType.ID = Convert.ToInt32(group.Key);
            Instance.TypesOfAds[firstType.ID] = firstType;
            foreach (var adType in group.Skip(1))
            {
                firstType.JoinType(adType);
                Instance.TypesOfAds.Remove(adType.ID);
            }
        }
    }
}
