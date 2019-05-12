using InstanceGenerator.InstanceData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGenerator.Interfaces
{
    public interface IMove
    {
        int IntegrityChange { get; }
        double ScoreChange { get; }
        Solution Solution { get; set; }
        Instance Instance { get; set; }
        void Execute();
        void RollBack();
        
    }
}
