using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ExampleFileReader.InstanceData.Activities
{
    public class Advertisement : BaseActivity
    {
        private BlockOfAds _block;
        private TypeOfAd _type;
        private Channel _channel;

        [XmlIgnore]
        public BlockOfAds Block
        {
            get => _block;
            set
            {
                _block = value;
                _block.AddAdvertisement(this);
            }
        }

        private string _blockID;
        public string BlockID
        {
            get
            {
                if(Block != null)
                {
                    return Block.ID;
                }
                return _blockID;
            }
            set
            {
                _blockID = value;
            }
        }
        
        public TypeOfAd Type
        {
            get => _type;
            set
            {
                _type = value;
                _type.AddAdvertisement(this);
            }
        }

        private string _typeID;
        public string TypeID
        {
            get
            {
                if (Type != null)
                {
                    return Type.ID;
                }
                return _typeID;
            }
            set
            {
                _typeID = value;
            }
        }

        [JsonIgnore]
        public Channel Channel
        {
            get => _channel;
            set
            {
                _channel = value;
            }
        }

        public decimal Cost { get; set; }
    }
}
