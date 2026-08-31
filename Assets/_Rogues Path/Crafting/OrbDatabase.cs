using System;
using System.Collections.Generic;
using System.Linq;
using _Rogues_Path._Game;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Rogues_Path.Crafting {
    [Serializable]
    public class WeightedOrb {
        public Orb Orb;

        [Min(1)]
        public int Weight = 100;
    }

    [CreateAssetMenu(menuName = Game.Name + "/" + nameof(OrbDatabase), fileName = nameof(OrbDatabase))]
    public class OrbDatabase : ScriptableObject {
        private static OrbDatabase m_Instance;

        public static OrbDatabase Instance {
            get {
                if (m_Instance == null) {
                    m_Instance = Resources.Load<OrbDatabase>("Databases/OrbDatabase");
                }

                return m_Instance;
            }
        }

        public List<WeightedOrb> Orbs => new(orbs);

        [SerializeField]
        public List<WeightedOrb> orbs = new();

        public Orb GetRandomOrb() {
            if (Orbs == null || Orbs.Count == 0)
                return null;

            int totalWeight = 0;

            foreach (WeightedOrb weightedOrb in Orbs) {
                if (weightedOrb?.Orb == null)
                    continue;

                totalWeight += weightedOrb.Weight;
            }

            if (totalWeight <= 0)
                return null;

            int roll = Random.Range(0, totalWeight);

            foreach (WeightedOrb weightedOrb in Orbs) {
                if (weightedOrb?.Orb == null)
                    continue;

                if (roll < weightedOrb.Weight)
                    return weightedOrb.Orb;

                roll -= weightedOrb.Weight;
            }

            return null;
        }

        public static bool TryGetByID(int id, out Orb orb) {
            orb = null;

            if (Instance == null)
                return false;

            if (id < 0 || id >= Instance.orbs.Count) {
                Debug.LogError($"Failed to find Orb by ID: {id}");
                return false;
            }

            orb = Instance.orbs[id]?.Orb;

            return orb != null;
        }

        public bool TryGetID(Orb orb, out int id) {
            id = -1;

            if (orb == null)
                return false;

            for (int i = 0; i < Orbs.Count; i++) {
                if (Orbs[i]?.Orb != orb)
                    continue;

                id = i;

                return true;
            }

            return false;
        }

        public bool TryFindByName(string orbName, out Orb orb) {
            orb = null;

            if (string.IsNullOrEmpty(orbName))
                return false;

            foreach (WeightedOrb weightedOrb in Orbs) {
                if (weightedOrb?.Orb == null)
                    continue;

                if (weightedOrb.Orb.Name != orbName)
                    continue;

                orb = weightedOrb.Orb;

                return true;
            }

            return false;
        }
    }
}