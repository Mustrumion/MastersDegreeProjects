using InstanceGenerator.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers.Solvers
{
    public class Evolutionary : BaseSolver
    {
        public ISolver GenerationCreator { get; set; }

        protected override void InternalSolve()
        {
        }
    }
}
