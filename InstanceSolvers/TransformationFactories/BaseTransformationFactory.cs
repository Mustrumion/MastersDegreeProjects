using InstanceGenerator.InstanceData;
using InstanceGenerator.SolutionObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceSolvers.TransformationFactories
{
    public abstract class BaseTransformationFactory
    {
        protected int _seed;
        protected Solution _solution;
        protected double _currentChange;

        public double RampUpSpeed { get; set; } = 1.0;
        [JsonIgnore]
        public int MaxMovesReturned = 10000;
        [JsonIgnore]
        public Random Random { get; set; }
        [JsonIgnore]
        public int Seed
        {
            get => _seed;
            set
            {
                Random = new Random(value);
                _seed = value;
            }
        }

        public bool MildlyRandomOrder { get; set; }

        [JsonIgnore]
        public Instance Instance { get; set; }

        [JsonIgnore]
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
            _currentChange += alpha * RampUpSpeed;
            if (_currentChange >= 1)
            {
                int step = Convert.ToInt32(_currentChange);
                _currentChange -= step;
                ChangeParametersBy(step);
            }
        }

        public void NarrowNeighborhood(double alpha)
        {
            _currentChange -= alpha * RampUpSpeed;
            if (_currentChange <= -1)
            {
                int step = -1 * Convert.ToInt32(Math.Abs(_currentChange));
                _currentChange -= step;
                ChangeParametersBy(step);
            }
        }


        protected abstract void ChangeParametersBy(int step);
    }
}
