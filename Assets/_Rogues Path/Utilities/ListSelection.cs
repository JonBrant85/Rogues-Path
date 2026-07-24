using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Rogues_Path.Utilities {
    public static class ListSelection {
        private static System.Random _random = new System.Random();

        public static T GetRandomElement<T>(this IList<T> collection) {
            int count = collection.Count();

            if (count == 0) {
                return default;
            }

            return collection.ElementAt(_random.Next(0, count));
        }

        public static T GetMiddleElement<T>(this IList<T> collection) {
            if (collection.Count == 0) {
                return default;
            }

            Debug.Log($"Collection.Count = {collection.Count}, Count/2 = {collection.Count / 2})");
            return collection[collection.Count / 2];
        }
    }
}