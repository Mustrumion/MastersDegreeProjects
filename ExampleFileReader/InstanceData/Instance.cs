﻿using ExampleFileReader.InstanceData.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleFileReader.InstanceData
{
    public class Instance
    {
        public Dictionary<string, Channel> Channels = new Dictionary<string, Channel>();
        public IEnumerable<Channel> GetChannelList()
        {
            return Channels.Values;
        }

        public Dictionary<string, BlockOfAds> BlocksOfAds = new Dictionary<string, BlockOfAds>();
        public IEnumerable<BlockOfAds> GetBlocksOfAdsList()
        {
            return BlocksOfAds.Values;
        }
        public BlockOfAds GetOrAddBlockOfAds(string blockId)
        {
            if (BlocksOfAds.ContainsKey(blockId))
            {
                return BlocksOfAds[blockId];
            }
            BlockOfAds block = new BlockOfAds()
            {
                ID = blockId,
            };
            BlocksOfAds[blockId] = block;
            return block;
        }

        public Dictionary<string, TypeOfAd> TypesOfAds = new Dictionary<string, TypeOfAd>();
        public IEnumerable<TypeOfAd> GetTypesOfAdsList()
        {
            return TypesOfAds.Values;
        }
        public TypeOfAd GetOrAddTypeOfAds(string blockId)
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



        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public int ChannelAmountChecksum { get; set; }
        public int ProgramAmountChecksum { get; set; }
        public int AdsAmountChecksum { get; set; }
    }
}
