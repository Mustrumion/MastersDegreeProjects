using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGenerator.InstanceData
{
    public class ViewershipFunction
    {
        public List<ViewershipSpan> TimeIntervals { get; set; } = new List<ViewershipSpan>();
        public string TypeID { get; set; }


        public double GetViewers(int unitsFromStart)
        {
            int unitsPassed = 0;
            foreach(var span in TimeIntervals)
            {
                unitsPassed += span.SpanUnits;
                if(unitsFromStart <= unitsPassed)
                {
                    return span.Viewers;
                }
            }
            return TimeIntervals.Last().Viewers;
        }


        internal void AddTimeInterval(ViewershipSpan span)
        {
            if(TimeIntervals.Count == 0)
            {
                TimeIntervals.Add(span);
                return;
            }
            ViewershipSpan previous = TimeIntervals.Last();
            if (previous.Viewers == span.Viewers || span.Viewers == 0 || previous.Viewers == 0)
            {
                previous.SpanUnits += span.SpanUnits;
                previous.Span += span.Span;
                previous.EndTime += span.Span;
                if(previous.Viewers == 0)
                {
                    previous.Viewers = span.Viewers;
                }
                return;
            }
            TimeIntervals.Add(span);
        }
    }
}
