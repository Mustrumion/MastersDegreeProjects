using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstanceGenerator
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

        /// <summary>
        /// Random shuffle based on Knuth algorithm
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">List to shuffle.</param>
        /// <param name="rand">Random number generator.</param>
        public static void Shuffle<T>(this IList<T> list, Random rand)
        {
            for (var i = 0; i < list.Count - 1; i++)
                list.Swap(i, rand.Next(i, list.Count));
        }

        public static void Swap<T>(this IList<T> list, int i, int j)
        {
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}
