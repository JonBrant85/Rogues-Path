using _Rogues_Path._Game;
using UnityEngine;

namespace _Rogues_Path.World {
    [CreateAssetMenu(
        menuName = Game.Name + "/" + nameof(WorldProgressionSettings),
        fileName = nameof(WorldProgressionSettings))]
    public class WorldProgressionSettings : ScriptableObject {
        private static WorldProgressionSettings m_Instance;

        public static WorldProgressionSettings Instance {
            get {
                if (m_Instance == null) {
                    m_Instance = Resources.Load<WorldProgressionSettings>(
                        "Databases/WorldProgressionSettings");
                }

                return m_Instance;
            }
        }

        [Min(0f)] public float EnemyHealthPerTraversal = 0.20f;
        [Min(0f)] public float EnemyStatPerTraversal = 0.10f;
        [Min(0)] public int RestEncountersPerGeneration = 2;
        [Min(0)] public int TreasureEncountersPerGeneration = 1;

        public float GetEnemyHealthMultiplier(int completedTraversals) {
            return 1f + Mathf.Max(0, completedTraversals)
                * EnemyHealthPerTraversal;
        }

        public float GetEnemyStatMultiplier(int completedTraversals) {
            return 1f + Mathf.Max(0, completedTraversals)
                * EnemyStatPerTraversal;
        }
    }
}
