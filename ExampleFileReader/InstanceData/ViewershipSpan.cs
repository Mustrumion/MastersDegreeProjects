using ExampleFileReader.InstanceData.Activities;
using ExampleFileReader.InstanceData.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleFileReader.InstanceData
{
    public class ViewershipSpan : ISpannedObject
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Span { get; set; }
        public int SpanUnits { get; set; }
        public double Viewers { get; set; }

        public ViewershipSpan() { }

        public ViewershipSpan(Autopromotion autopromotion)
        {
            StartTime = autopromotion.StartTime;
            EndTime = autopromotion.EndTime;
            Span = autopromotion.Span;
            SpanUnits = autopromotion.SpanUnits;
            Viewers = 0.0d;
        }

        public ViewershipSpan(AdvertisementInstance advertisement)
        {
            StartTime = advertisement.StartTime;
            EndTime = advertisement.EndTime;
            Span = advertisement.Span;
            SpanUnits = advertisement.SpanUnits;
            Viewers = advertisement.Viewers;
        }
    }
}
