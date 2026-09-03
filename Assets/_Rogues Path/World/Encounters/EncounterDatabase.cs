using System;
using System.Collections.Generic;
using _Rogues_Path._Game;
using UnityEngine;

namespace _Rogues_Path.World.Encounters {
    [Serializable]
    public class WeightedEncounter {
        public EncounterData Encounter;

        [Min(1)]
        public int Weight = 1;
    }

    [CreateAssetMenu(menuName = Game.Name + "/" + nameof(EncounterDatabase), fileName = nameof(EncounterDatabase))]
    public class EncounterDatabase : ScriptableObject {
        private static EncounterDatabase m_Instance;

        public static EncounterDatabase Instance {
            get {
                if (m_Instance == null) {
                    m_Instance = Resources.Load<EncounterDatabase>("Databases/EncounterDatabase");
                }

                return m_Instance;
            }
        }

        [SerializeField]
        private List<WeightedEncounter> encounters = new();

        public static bool TryGetByID(int id, out EncounterData encounter) {
            encounter = null;

            if (Instance == null || id < 0 || id >= Instance.encounters.Count)
                return false;

            encounter = Instance.encounters[id]?.Encounter;

            return encounter != null;
        }

        public static bool TryGetRandomID(out int id) {
            return TryGetRandomID<EncounterData>(out id);
        }

        public static bool TryGetRandomID<TEncounter>(out int id)
            where TEncounter : EncounterData {

            id = -1;

            if (Instance == null)
                return false;

            int totalWeight = 0;

            foreach (WeightedEncounter entry in Instance.encounters) {
                if (entry?.Encounter is TEncounter && entry.Weight > 0)
                    totalWeight += entry.Weight;
            }

            if (totalWeight <= 0) {
                Debug.LogError(
                    $"{Instance.name} contains no valid weighted "
                    + $"{typeof(TEncounter).Name} encounters.");

                return false;
            }

            int roll = UnityEngine.Random.Range(0, totalWeight);

            for (int i = 0; i < Instance.encounters.Count; i++) {
                WeightedEncounter entry = Instance.encounters[i];

                if (entry?.Encounter is not TEncounter || entry.Weight <= 0)
                    continue;

                if (roll < entry.Weight) {
                    id = i;
                    return true;
                }

                roll -= entry.Weight;
            }

            return false;
        }
    }
}
