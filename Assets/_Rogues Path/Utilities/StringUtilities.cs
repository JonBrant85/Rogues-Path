using System.Collections.Generic;

namespace _Rogues_Path.Utilities {
    public static class StringUtilities {
        public static string ToCommaDelimitedString(this List<int> list, bool useSpaces = true) {
            string commaDelimitedString = "";

            for (int i = 0; i < list.Count; i++) {
                commaDelimitedString += list[i];

                if (i+1 < list.Count) {
                    commaDelimitedString += useSpaces ? ", " : ",";
                }
            }

            return commaDelimitedString;
        }
    }
}