using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waypoint
{
    /// <summary>
    /// Static class containing extension methods for the project.
    /// </summary>
    public static class ExtensionMethods
    {
        // Take a list and return a list of all the unique pairs of items in it
        public static List<Tuple<T, T>> Pairs<T>(this List<T> list)
        {
            List<Tuple<T, T>> pairs = new List<Tuple<T, T>>();
            for (int i = 0; i < list.Count; ++i)
            {
                for (int j = i + 1; j < list.Count; ++j)
                {
                    pairs.Add(new Tuple<T, T>(list[i], list[j]));
                }
            }

            return pairs;
        }
    }
}
