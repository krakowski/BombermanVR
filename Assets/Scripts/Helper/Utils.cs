using System;
using System.Collections.Generic;

/// <summary>
///     The Utils class provides methods for shuffling Lists and Arrays.
/// </summary>
public class Utils {

    private class RandomComparer<T> : IComparer<T> {

        private System.Random rnd;

        public RandomComparer(int seed) {
            rnd = new System.Random(seed);
        }

        public int Compare(T x, T y) {
            return rnd.Next(-1, 1);
        }
    }

    public static T[] shuffle<T>(T[] array, int seed) {
        T[] result = new T[array.Length];
        array.CopyTo(result,0);
        Array.Sort<T>(result, new RandomComparer<T>(seed));
        return result;
    }

    public static void shuffle<T>(List<T> list, int seed) {
        list.Sort(new RandomComparer<T>(seed));
    }
}
