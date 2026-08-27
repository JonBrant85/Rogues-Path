using System.Collections.Generic;
using System.Linq;
using _Rogues_Path._Game;
using UnityEngine;

namespace _Rogues_Path.Crafting {
    [CreateAssetMenu(
        menuName = Game.Name + "/" + nameof(OrbDatabase),
        fileName = nameof(OrbDatabase))]
    public class OrbDatabase : ScriptableObject {
        private static OrbDatabase m_Instance;

        public static OrbDatabase Instance {
            get {
                if (m_Instance == null) {
                    m_Instance = Resources.Load<OrbDatabase>(
                        "Databases/OrbDatabase");
                }

                return m_Instance;
            }
        }

        public List<Orb> Orbs => new(orbs);

        [SerializeField]
        private List<Orb> orbs = new();

        public static bool TryGetByID(int id, out Orb orb) {
            orb = null;

            if (Instance == null)
                return false;

            if (id < 0 || id >= Instance.orbs.Count) {
                Debug.LogError($"Failed to find Orb by ID: {id}");
                return false;
            }

            orb = Instance.orbs[id];

            return orb != null;
        }

        public static bool TryGetID(Orb orb, out int id) {
            id = -1;

            if (orb == null || Instance == null)
                return false;

            id = Instance.orbs.IndexOf(orb);

            return id >= 0;
        }

        public static bool TryFindByName(string name, out Orb orb) {
            orb = null;

            if (Instance == null || string.IsNullOrEmpty(name))
                return false;

            orb = Instance.orbs.FirstOrDefault(
                x => x != null && x.Name == name);

            return orb != null;
        }
    }
}