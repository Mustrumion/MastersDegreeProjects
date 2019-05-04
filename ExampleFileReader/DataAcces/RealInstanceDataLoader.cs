using ExampleFileReader.InstanceData;
using ExampleFileReader.InstanceData.Activities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleFileReader.DataAccess
{
    public class RealInstanceDataLoader
    {
        /// <summary>
        /// Path to the real data instance. Used only if Reader is not set.
        /// </summary>
        public string Filepath { get; set; }
        /// <summary>
        /// Reader with real instance data.
        /// </summary>
        public TextReader Reader { get; set; }


        private Instance instance;
        private Channel currentChannel;
        private int lineNum;

        private static readonly string DATETIME_FORMAT_HEADER = "d.M.yyyy H:m:s";
        private static readonly string DATETIME_FORMAT_CHANNEL = "yyyy-MM-dd HH:mm:ss";



        public Instance LoadInstanceFile()
        {
            instance = new Instance();
            using (Reader = Reader ?? new StreamReader(Filepath))
            {
                try
                {
                    ReadInstancesHeader();
                    ReadInstanceChecksums();
                    for(int i = 0; i < instance.ChannelAmountChecksum; i++)
                    {
                        ReadChannelAssignment();
                    }
                }
                catch(Exception e)
                {
                    throw new Exception($"Error at line {lineNum}. {e.Message}.", e);
                }
            }
            return instance;
        }

        private void ReadInstancesHeader()
        {
            lineNum += 1;
            string channelLine = Reader.ReadLine();
            if (string.IsNullOrEmpty(channelLine))
            {
                throw new IOException("There are no channels in the first line.");
            }
            string[] channelIds = channelLine.Split(' ');
            foreach (string channel in channelIds)
            {
                instance.Channels.Add(channel, new Channel()
                {
                    ID = channel,
                });
            }
            lineNum += 1;
            string start = Reader.ReadLine();
            instance.StartTime = DateTime.ParseExact(start, DATETIME_FORMAT_HEADER, CultureInfo.InvariantCulture);
            lineNum += 1;
            string end = Reader.ReadLine();
            instance.EndTime = DateTime.ParseExact(end, DATETIME_FORMAT_HEADER, CultureInfo.InvariantCulture);
            lineNum += 1;
            string intermissionLine = Reader.ReadLine();
            if (!intermissionLine.StartsWith("%"))
            {
                throw new Exception($"Unexpected sequence encountered at the end of instance header - '{intermissionLine}'. Expected '%'.");
            }
        }

        private void ReadInstanceChecksums()
        {
            lineNum += 1;
            string channelNumber = Reader.ReadLine();
            instance.ChannelAmountChecksum = Convert.ToInt32(channelNumber);
            lineNum += 1;
            string programNumber = Reader.ReadLine();
            instance.ProgramAmountChecksum = Convert.ToInt32(programNumber);
            lineNum += 1;
            string adsNumber = Reader.ReadLine();
            instance.AdsAmountChecksum = Convert.ToInt32(adsNumber);
            string intermissionLine = Reader.ReadLine();
            if (!intermissionLine.StartsWith("%"))
            {
                throw new Exception($"Unexpected sequence encountered at the end of instance checksums - '{intermissionLine}'. Expected '%'.");
            }
        }

        private void ReadChannelAssignment()
        {
            lineNum += 1;
            string channelID = Reader.ReadLine();
            currentChannel = instance.Channels[channelID];
            lineNum += 1;
            string channelStart = Reader.ReadLine();
            currentChannel.StartTime = DateTime.ParseExact(channelStart, DATETIME_FORMAT_CHANNEL, CultureInfo.InvariantCulture);
            currentChannel.EndTime = currentChannel.StartTime;
            string line = null;
            lineNum += 1;
            while (!(line = Reader.ReadLine()).StartsWith("%"))
            {
                if (line.StartsWith("R"))
                {
                    AnalyzeAd(line);
                }
                else if (line.StartsWith("P"))
                {
                    AnalyzeProgram(line);
                }
                else if (line.StartsWith("A"))
                {
                    AnalyzeAutopromotion(line);
                }
                lineNum += 1;
            }
        }

        private void AnalyzeAutopromotion(string line)
        {
            string[] fields = line.Split(' ');
            Autopromotion autopromotion = new Autopromotion()
            {
                Span = new TimeSpan(0, 0, Convert.ToInt32(fields[1])),
                StartTime = currentChannel.EndTime,
            };
            autopromotion.EndTime = currentChannel.EndTime = currentChannel.EndTime.Add(autopromotion.Span);
            currentChannel.Autopromotions.Add(autopromotion);
            currentChannel.Activities.Add(autopromotion);
        }

        private void AnalyzeProgram(string line)
        {
            string[] fields = line.Split(' ');
            TvProgram program = new TvProgram()
            {
                Span = new TimeSpan(0, 0, Convert.ToInt32(fields[1])),
                StartTime = currentChannel.EndTime,
            };
            program.EndTime = currentChannel.EndTime = currentChannel.EndTime.Add(program.Span);
            currentChannel.Programs.Add(program);
            currentChannel.Activities.Add(program);
        }

        private void AnalyzeAd(string line)
        {
            string[] fields = line.Split(' ');
            AdvertisementInstance advertisement = new AdvertisementInstance()
            {
                Span = new TimeSpan(0, 0, Convert.ToInt32(fields[1])),
                Type = instance.GetOrAddTypeOfAds(fields[4]),
                Profit = Convert.ToDouble(fields[5].Replace(",", "."), CultureInfo.InvariantCulture),
                AdvertisementOrder = instance.GetOrAddOrderOfAds(fields[7]),
                Owner = instance.GetOrAddOwnerOfAds(fields[8]),
                Viewers = Convert.ToDouble(fields[9].Replace(",", "."), CultureInfo.InvariantCulture),
                Channel = currentChannel,
                StartTime = currentChannel.EndTime,
            };
            advertisement.EndTime = currentChannel.EndTime = currentChannel.EndTime.Add(advertisement.Span);
            currentChannel.Advertisements.Add(advertisement);
            currentChannel.Activities.Add(advertisement);
        }
    }
}
