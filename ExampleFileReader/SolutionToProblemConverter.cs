using ExampleFileReader.InstanceData;
using ExampleFileReader.InstanceData.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleFileReader
{
    public class SolutionToProblemConverter
    {
        public Instance Instance { get; set; }
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
                        currentBreak.EndTime = program.Start;
                        channel.AddBreak(currentBreak);
                    }
                    currentBreak = new TvBreak()
                    {
                        StartTime = program.End,
                    };
                }
                if (activity is AdvertisementInstance)
                {
                    if (currentBreak == null)
                    {
                        currentBreak = new TvBreak()
                        {
                            StartTime = activity.Start,
                        };
                    }
                    currentBreak.AddAdvertisement(activity as AdvertisementInstance);
                }
            }
            if (currentBreak != null && currentBreak.Advertisements.Count > 0)
            {
                currentBreak.EndTime = currentBreak.Advertisements.Last().End;
                channel.AddBreak(currentBreak);
            }
        }
    }
}
