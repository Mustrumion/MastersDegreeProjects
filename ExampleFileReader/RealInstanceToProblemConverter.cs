using ExampleFileReader.InstanceData;
using ExampleFileReader.InstanceData.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleFileReader
{
    public class RealInstanceToProblemConverter
    {
        public Instance Instance { get; set; }
        public double TimeUnitInSeconds = 3.0d;


        public void ConvertToProblem()
        {
            CreateBreaks();
            RemoveUnnecessaryActivities();
            ConvertToUnits();
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
                    if (currentBreak != null && currentBreak.Advertisements.Count > 0)
                    {
                        currentBreak.EndTime = program.StartTime;
                        channel.AddBreak(currentBreak);
                    }
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
                    if(activity is AdvertisementInstance)
                    {
                        currentBreak.AddAdvertisement(activity as AdvertisementInstance);
                    }
                }
            }
            if (currentBreak != null && currentBreak.Advertisements.Count > 0)
            {
                currentBreak.EndTime = currentBreak.Advertisements.Last().EndTime;
                channel.AddBreak(currentBreak);
            }
        }


        public void RemoveUnnecessaryActivities()
        {
            foreach(var channel in Instance.Channels.Values)
            {
                channel.Autopromotions.Clear();
                channel.Programs.Clear();
                channel.Activities.RemoveAll(a => a is TvProgram || a is Autopromotion);
            }
        }

        public void ConvertToUnits()
        {
            Instance.UnitSizeInSeconds = TimeUnitInSeconds;
            TimeSpan instanceSpan = Instance.EndTime - Instance.StartTime;
            double instanceSpanSeconds = Helpers.NearestDivisibleBy(instanceSpan.TotalSeconds, TimeUnitInSeconds, out int numberOfUnits);
            Instance.Span = new TimeSpan(0, 0, Convert.ToInt32(instanceSpanSeconds));
            Instance.SpanUnits = numberOfUnits;
            Instance.EndTime = Instance.StartTime + Instance.Span;
            foreach (var channel in Instance.Channels.Values)
            {
                TimeSpan span = channel.EndTime - channel.StartTime;
                double spanSeconds = Helpers.NearestDivisibleBy(span.TotalSeconds, TimeUnitInSeconds, out int number);
                channel.Span = new TimeSpan(0, 0, Convert.ToInt32(spanSeconds));
                channel.SpanUnits = number;
                channel.EndTime = channel.StartTime + channel.Span;
                ConvertBreaksToUnits(channel);
                ConvertAdInstancesToUnits(channel);
            }

        }

        private void ConvertBreaksToUnits(Channel channel)
        {
            foreach (var adBreak in channel.Breaks)
            {
                TimeSpan span = adBreak.EndTime - adBreak.StartTime;
                double spanSeconds = Helpers.NearestDivisibleBy(span.TotalSeconds, TimeUnitInSeconds, out int number);
                adBreak.DesiredTimespan = new TimeSpan(0, 0, Convert.ToInt32(spanSeconds));
                adBreak.DesiredTimespanUnits = number;
                adBreak.EndTime = adBreak.StartTime + adBreak.DesiredTimespan;
            }
        }

        private void ConvertAdInstancesToUnits(Channel channel)
        {
            foreach (var advertisement in channel.Advertisements)
            {
                TimeSpan span = advertisement.EndTime - advertisement.StartTime;
                double spanSeconds = Helpers.NearestDivisibleBy(span.TotalSeconds, TimeUnitInSeconds, out int number);
                advertisement.Span = new TimeSpan(0, 0, Convert.ToInt32(spanSeconds));
                advertisement.SpanUnits = number;
                advertisement.EndTime = advertisement.StartTime + advertisement.Span;
            }
        }
    }
}
