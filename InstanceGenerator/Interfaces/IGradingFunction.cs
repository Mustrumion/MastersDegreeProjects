using InstanceGenerator.InstanceData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGenerator.Interfaces
{
    public interface IScoringFunction
    {
        Solution Solution { get; set; }
        Instance Instance { get; set; }
        string Description { get; set; }
    }
}
