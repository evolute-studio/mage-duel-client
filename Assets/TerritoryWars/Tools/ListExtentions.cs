using System;
using System.Collections.Generic;

namespace TerritoryWars.Tools
{
    public static class ListExtensions
    {
        private static Random rng = new Random();
        
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                int k = rng.Next(n--);
                (list[n], list[k]) = (list[k], list[n]);
            }
        }
    }
}


