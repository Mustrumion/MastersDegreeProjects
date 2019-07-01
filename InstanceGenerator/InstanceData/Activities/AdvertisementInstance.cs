using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace InstanceGenerator.InstanceData.Activities
{
    public class AdvertisementInstance : BaseActivity
    {
        private AdvertisementTask _adOrder;
        private TypeOfAd _type;
        private Brand _brand;

        [JsonProperty(Order = -2)]
        public double Profit { get; set; }
        [JsonProperty(Order = -1)]
        public double Viewers { get; set; }

        [XmlIgnore]
        public AdvertisementTask AdvertisementOrder
        {
            get => _adOrder;
            set
            {
                _adOrder = value;
                if (!_adOrder.AdvertisementInstances.Contains(this))
                {
                    _adOrder.AddAdvertisement(this);
                }
            }
        }

        private int _adOrderID;
        public int AdOrderID
        {
            get
            {
                if (AdvertisementOrder != null)
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

        public Brand Brand
        {
            get => _brand;
            set
            {
                _brand = value;
                if (!_brand.Ads.Contains(this))
                {
                    _brand.AddAdvertisement(this);
                }
            }
        }


        private int _ownerID;
        public int OwnerID
        {
            get
            {
                if (Brand != null)
                {
                    return Brand.ID;
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

        private int _typeID;
        private TvBreak _break;

        public int TypeID
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
        public Channel Channel { get; set; }

        [JsonIgnore]
        public TvBreak Break
        {
            get => _break;
            set
            {
                _break = value;
                if (!_break.Advertisements.Contains(this))
                {
                    _break.AddAdvertisement(this);
                }
            }
        }
    }
}
