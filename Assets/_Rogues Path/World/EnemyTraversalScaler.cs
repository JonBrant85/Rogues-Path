using System.Collections.Generic;
using _Rogues_Path.Pawns.Scripts;
using _Rogues_Path.UI.CharacterScreen;
using Kryz.CharacterStats;
using UnityEngine;

namespace _Rogues_Path.World {
    public static class EnemyTraversalScaler {
        public static bool TryApply(
            Pawn enemy,
            int completedTraversals,
            WorldProgressionSettings settings) {

            if (enemy == null) {
                Debug.LogError("Cannot scale a null enemy.");
                return false;
            }

            if (settings == null) {
                Debug.LogError(
                    "Cannot scale enemy without WorldProgressionSettings.");
                return false;
            }

            if (enemy.Stats == null
                || !enemy.Stats.TryGetValue(
                    enemy.MaxHealthID,
                    out CharacterStat maximumHealth)) {

                Debug.LogError(
                    $"{enemy.CharacterName} has no maximum-health stat.");
                return false;
            }

            float healthMultiplier =
                settings.GetEnemyHealthMultiplier(completedTraversals);
            float statMultiplier =
                settings.GetEnemyStatMultiplier(completedTraversals);

            foreach (KeyValuePair<CharacterStatID, CharacterStat> stat in enemy.Stats) {
                if (stat.Value == null)
                    continue;

                float multiplier = stat.Key == enemy.MaxHealthID
                    ? healthMultiplier
                    : statMultiplier;

                stat.Value.BaseValue *= multiplier;
            }

            enemy.CurrentHealth = maximumHealth.Value;

            return true;
        }
    }
}
