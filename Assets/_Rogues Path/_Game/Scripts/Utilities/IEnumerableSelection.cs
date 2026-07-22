using System.Collections.Generic;
using System.Linq;
using _Rogues_Path._Game;

namespace _Rogues_Path.Utilities {
    public static class IEnumerableSelection {
        private static System.Random _random = new System.Random();

        public static T GetRandomElement<T>(this IEnumerable<T> collection) {
            int count = collection.Count();

            if (count == 0) {
                return default;
            }

            return collection.ElementAt(_random.Next(0, count));
        }

        public static T GetMiddleElement<T>(this IEnumerable<T> collection) {
            int count = collection.Count();

            if (count == 0) {
                return default;
            }

            //Handle evens
            if (count % 2 == 0) {
                return collection.ElementAt((count / 2) - 1);
            }
            //Handle odds
            else {
                return collection.ElementAt(count / 2);
            }

        }
    }
}