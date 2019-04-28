using ExampleFileReader.InstanceData;
using ExampleFileReader.InstanceData.Activities;
using ExampleFileReader.InstanceData.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleFileReader.InstanceModification
{
    public class RealInstanceToProblemConverter
    {
        public Instance Instance { get; set; }
        public double TimeUnitInSeconds = 3.0d;


        public void ConvertToProblem()
        {
            ConvertToUnits();
            InstanceCorrector corrector = new InstanceCorrector() { Instance = Instance };
            corrector.AdjustTimestampsToDuration();
            CreateBreaks();
            AddViewershipFunctions();
            RemoveUnnecessaryActivities();
        }

        public void CreateBreaks()
        {
            foreach(Channel channel in Instance.GetChannelList())
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
    }
}
