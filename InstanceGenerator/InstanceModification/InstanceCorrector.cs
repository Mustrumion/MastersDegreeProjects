using InstanceGenerator.InstanceData;
using InstanceGenerator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGenerator.InstanceModification
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
                AdjustStartTime(channel, Instance.StartTime);
                AdjustLengthAndActivities(channel);
                foreach(var tvBreak in channel.Breaks)
                {
                    AdjustStartTime(tvBreak, channel.StartTime);
                    AdjustLengthAndActivities(tvBreak);
                }
            }
        }

        public void AdjustAdViewsToCurrentPositionViews()
        {
            foreach(var tvBreak in Instance.Breaks.Values)
            {
                int currentUnit = 0;
                foreach(var adInstance in tvBreak.Advertisements)
                {
                    var function = tvBreak.GetViewsFuntion(adInstance.TypeID);
                    adInstance.Viewers = function.GetViewers(currentUnit);
                    currentUnit += adInstance.SpanUnits;
                }
            }
        }


        public void AdjustAdLengthToChosenOrderLength()
        {
            foreach (var tvBreak in Instance.Breaks.Values)
            {
                foreach (var adInstance in tvBreak.Advertisements)
                {
                    var function = tvBreak.GetViewsFuntion(adInstance.TypeID);
                    adInstance.SpanUnits = adInstance.AdvertisementOrder.AdSpanUnits;
                    adInstance.Span = adInstance.AdvertisementOrder.AdSpan;
                }
            }
        }


        private void AdjustStartTime(IActivitiesSequence activitiesContainer, DateTime earliestPossibleStart)
        {
            if (activitiesContainer.StartTime < earliestPossibleStart)
            {
                activitiesContainer.StartTime = earliestPossibleStart;
            }

            TimeSpan span = activitiesContainer.StartTime - earliestPossibleStart;
            double spanSeconds = Helpers.NearestDivisibleBy(span.TotalSeconds, Instance.UnitSizeInSeconds, out int number);
            TimeSpan timeAfterStart = new TimeSpan(0, 0, Convert.ToInt32(spanSeconds));
            activitiesContainer.StartTime = earliestPossibleStart + timeAfterStart;
            var activity = activitiesContainer.Activities.FirstOrDefault();
            if(activity != null)
            {
                activity.StartTime = activitiesContainer.StartTime;
                activity.EndTime = activity.StartTime + activity.Span;
            }
        }

        private void AdjustLengthAndActivities(IActivitiesSequence activitiesContainer)
        {
            // adjust channel start times to be equal or later to instance/channel start and 
            activitiesContainer.Activities[0].StartTime = activitiesContainer.StartTime;
            for (int i = 1; i < activitiesContainer.Activities.Count; i++)
            {
                var firstActiv = activitiesContainer.Activities[i - 1];
                var secondActiv = activitiesContainer.Activities[i];
                if (secondActiv.StartTime < firstActiv.EndTime)
                {
                    secondActiv.StartTime = firstActiv.EndTime;
                    secondActiv.EndTime = secondActiv.StartTime + secondActiv.Span;
                }
            }
            if (activitiesContainer.Activities.Count > 0)
            {
                activitiesContainer.EndTime = activitiesContainer.Activities.Last().EndTime;
                activitiesContainer.Span = activitiesContainer.EndTime - activitiesContainer.StartTime;
                if (Instance.UnitSizeInSeconds != 0)
                {
                    Helpers.NearestDivisibleBy(activitiesContainer.Span.TotalSeconds, Instance.UnitSizeInSeconds, out int numberOfUnits);
                    activitiesContainer.SpanUnits = numberOfUnits;
                }
            }
        }
    }
}
