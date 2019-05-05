using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleFileReader
{
    public static class Helpers
    {
        public static double NearestDivisibleBy(double rounded, double unit)
        {
            return NearestDivisibleBy(rounded, unit, out _);
        }

        public static double NearestDivisibleBy(double rounded, double unit, out int numberOfUnits)
        {
            numberOfUnits = Convert.ToInt32(rounded / unit);
            return unit * numberOfUnits;
        }
    }
}
