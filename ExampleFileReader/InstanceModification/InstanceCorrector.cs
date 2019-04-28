using ExampleFileReader.InstanceData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleFileReader.InstanceModification
{
    public class InstanceCorrector
    {
        public Instance Instance { get; set; }

        /// <summary>
        /// If activity durations were changed, they may overlap with another activities on cahnnel / during the break. This function corrects those inconsistencies.
        /// </summary>
        public void AdjustTimestampsToDuration()
        {
            foreach(var channel in Instance.Channels.Values)
            {
                AdjustChannelActivities(channel);
                foreach(var tvBreak in channel.Breaks)
                {
                    AdjustBreakActivities(tvBreak);
                }
            }
        }

        private void AdjustChannelActivities(Channel channel)
        {
            for (int i = 1; i < channel.Activities.Count; i++)
            {
                var firstActiv = channel.Activities[i - 1];
                var secondActiv = channel.Activities[i];
                if (secondActiv.StartTime < firstActiv.EndTime)
                {
                    secondActiv.StartTime = firstActiv.EndTime;
                    secondActiv.EndTime = secondActiv.StartTime + secondActiv.Span;
                }
            }
            if (channel.Activities.Count > 0)
            {
                channel.EndTime = channel.Activities.Last().EndTime;
                channel.Span = channel.EndTime - channel.StartTime;
                if (Instance.UnitSizeInSeconds != 0)
                {
                    Helpers.NearestDivisibleBy(channel.Span.TotalSeconds, Instance.UnitSizeInSeconds, out int numberOfUnits);
                    channel.SpanUnits = numberOfUnits;
                }
            }
        }

        private void AdjustBreakActivities(TvBreak tvBreak)
        {
            for (int i = 1; i < tvBreak.Activities.Count; i++)
            {
                var firstActiv = tvBreak.Activities[i - 1];
                var secondActiv = tvBreak.Activities[i];
                if (secondActiv.StartTime < firstActiv.EndTime)
                {
                    secondActiv.StartTime = firstActiv.EndTime;
                    secondActiv.EndTime = secondActiv.StartTime + secondActiv.Span;
                }
            }
            if (tvBreak.Activities.Count > 0)
            {
                tvBreak.EndTime = tvBreak.Activities.Last().EndTime;
                tvBreak.DesiredTimespan = tvBreak.EndTime - tvBreak.StartTime;
                if (Instance.UnitSizeInSeconds != 0)
                {
                    Helpers.NearestDivisibleBy(tvBreak.DesiredTimespan.TotalSeconds, Instance.UnitSizeInSeconds, out int numberOfUnits);
                    tvBreak.DesiredTimespanUnits = numberOfUnits;
                }
            }
        }
    }
}
