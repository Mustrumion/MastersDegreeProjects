using ExampleFileReader.InstanceData;
using ExampleFileReader.InstanceData.Interfaces;
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
                AdjustLengthAndActivities(channel);
                foreach(var tvBreak in channel.Breaks)
                {
                    AdjustLengthAndActivities(tvBreak);
                }
            }
        }

        private void AdjustLengthAndActivities(IActivitiesSequence activitiesContainer)
        {
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
