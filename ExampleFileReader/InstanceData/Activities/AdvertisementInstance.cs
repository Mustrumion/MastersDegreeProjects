using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ExampleFileReader.InstanceData.Activities
{
    public class AdvertisementInstance : BaseActivity
    {
        private AdvertisementOrder _adOrder;
        private TypeOfAd _type;
        private OwnerOfAd _owner;
        private Channel _channel;

        [XmlIgnore]
        public AdvertisementOrder AdvertisementOrder
        {
            get => _adOrder;
            set
            {
                _adOrder = value;
                _adOrder.AddAdvertisement(this);
            }
        }

        private string _adOrderID;
        public string AdOrderID
        {
            get
            {
                if(AdvertisementOrder != null)
                {
                    return AdvertisementOrder.ID;
                }
                return _adOrderID;
            }
            set
            {
                _adOrderID = value;
            }
        }

        public OwnerOfAd Owner
        {
            get => _owner;
            set
            {
                _owner = value;
                _owner.AddAdvertisement(this);
            }
        }


        private string _ownerID;
        public string OwnerID
        {
            get
            {
                if (Owner != null)
                {
                    return Owner.ID;
                }
                return _ownerID;
            }
            set
            {
                _ownerID = value;
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

        public double Cost { get; set; }
    }
}
