using InstanceGenerator.InstanceData;
using InstanceGenerator.SolutionObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers.MoveFactories
{
    public abstract class BaseMoveFactory
    {
        protected Solution _solution;
        protected double _currentChange;

        public Random Random { get; set; }
        public bool MildlyRandomOrder { get; set; }
        public Instance Instance { get; set; }

        public Solution Solution
        {
            get => _solution;
            set
            {
                _solution = value;
                if (_solution != null && Instance != _solution.Instance)
                {
                    Instance = Solution.Instance;
                }
            }
        }


        public void WidenNeighborhood(double alpha)
        {
            _currentChange += alpha;
            if (alpha >= 1)
            {
                int step = Convert.ToInt32(alpha);
                _currentChange -= step;
                ChangeParametersBy(step);
            }
        }

        public void NarrowNeighborhood(double alpha)
        {
            _currentChange -= alpha;
            if (alpha <= -1)
            {
                int step = -1 * Convert.ToInt32(Math.Abs(alpha));
                _currentChange -= step;
                ChangeParametersBy(step);
            }
        }


        protected abstract void ChangeParametersBy(int step);
    }
}
