using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleFileReader.InstanceData.Activities
{
    public class Advertisement : BaseActivity
    {
        private BlockOfAds _block;
        private TypeOfAd _type;
        private Channel _channel;

        public BlockOfAds Block
        {
            get => _block;
            set
            {
                _block = value;
                _block.AddAdvertisement(this);
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
