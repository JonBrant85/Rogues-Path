using System.Collections.Generic;
using _Rogues_Path._Game;
using _Rogues_Path.LevelSelection;
using UnityEngine;

namespace _Rogues_Path.Levels {
    [CreateAssetMenu(menuName = Game.Name + "/Databases/Level Database", fileName = "LevelDatabase")]
    public class LevelDatabase : ScriptableObject {
        private static LevelDatabase instance;

        public static LevelDatabase Instance {
            get {
                if (instance == null) {
                    instance = Resources.Load<LevelDatabase>("LevelDatabase");
                }

                return instance;
            }
        }

        public List<LevelData> Levels = new();

        public bool TryGetByID(int id, out LevelData level) {
            level = null;
            if (id < 0 || id >= Levels.Count)
                return false;

            level = Levels[id];
            return level != null;
        }

        public bool TryGetID(LevelData level, out int id) {
            id = Levels.IndexOf(level);
            return id >= 0;
        }

        public LevelData GetRandomLevel() {
            if (Levels == null || Levels.Count == 0) {
                Debug.LogError("LevelDatabase contains no LevelData.");

                return null;
            }

            return Levels[Random.Range(0, Levels.Count)];
        }
    }
}