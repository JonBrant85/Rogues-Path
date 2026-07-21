using System.Collections.Generic;
using System.Linq;

namespace _Rogues_Path.Utilities {
    public static class GetRandomElementClass {
        private static System.Random _random = new System.Random();

        public static T GetRandomElement<T>(this IEnumerable<T> collection) {
            int count = collection.Count();

            if (count == 0) {
                return default;
            }

            return collection.ElementAt(_random.Next(0, count));
        }
    }
}