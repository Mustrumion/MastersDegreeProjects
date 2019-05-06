using InstanceGenerator.InstanceData.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGenerator.InstanceData.Interfaces
{
    public interface IActivitiesSequence : ISpannedObject
    {
        List<BaseActivity> Activities { get; set; }
    }
}
