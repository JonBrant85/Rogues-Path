using DuloGames.UI;
using UnityEngine;

namespace _Rogues_Path.Equipment.Scripts {
    public static class EquipmentQualityRoller {
        private static readonly UIItemQuality[] Qualities = {
            UIItemQuality.Poor,
            UIItemQuality.Common,
            UIItemQuality.Uncommon,
            UIItemQuality.Rare,
            UIItemQuality.Epic,
            UIItemQuality.Legendary
        };

        // Defaults match the treasure encounter's quality distribution.
        public static UIItemQuality Roll(
            int poorWeight = 10,
            int commonWeight = 40,
            int uncommonWeight = 25,
            int rareWeight = 15,
            int epicWeight = 7,
            int legendaryWeight = 3) {

            int[] weights = {
                Mathf.Max(0, poorWeight),
                Mathf.Max(0, commonWeight),
                Mathf.Max(0, uncommonWeight),
                Mathf.Max(0, rareWeight),
                Mathf.Max(0, epicWeight),
                Mathf.Max(0, legendaryWeight)
            };

            long totalWeight = 0;
            foreach (int weight in weights)
                totalWeight += weight;

            if (totalWeight <= 0 || totalWeight > int.MaxValue) {
                Debug.LogError("Equipment quality weights have an invalid total. Defaulting to Poor.");
                return UIItemQuality.Poor;
            }

            int roll = Random.Range(0, (int)totalWeight);
            for (int i = 0; i < weights.Length; i++) {
                if (roll < weights[i])
                    return Qualities[i];

                roll -= weights[i];
            }

            return UIItemQuality.Poor;
        }
    }
}
