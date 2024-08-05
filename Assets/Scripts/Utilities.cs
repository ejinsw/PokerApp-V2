using System;
using System.Collections.Generic;
using Random = Unity.Mathematics.Random;

namespace Poker {
    public static class Utilities {
        private static Random rng = new Random((uint)Guid.NewGuid().GetHashCode());

        public static void Shuffle<T>(this IList<T> list) {
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = rng.NextInt(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}