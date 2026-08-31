using System;
using System.Collections.Generic;
using _Rogues_Path.Levels;
using _Rogues_Path.LevelSelection;
using _Rogues_Path.Pawns.Scripts;
using _Rogues_Path.World.Encounters;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Rogues_Path.World {
    public class WorldTile : MonoBehaviour {
        public LevelData Level;
        public EncounterData Encounter;
        public WorldTile NextTile;
        public SpriteRenderer IndicatorSprite;
        public Transform PawnContainer;
        public GameObject Model;
        [SerializeField] private Transform EnemyContainer;

        private PawnData enemy;

        private void Start() {
            GenerateEncounter();
        }

        private void GenerateEncounter() {
            if (EnemyContainer == null) {
                Debug.LogError($"{name}: EnemyContainer is not assigned.");
                return;
            }

            Level = LevelDatabase.Instance.GetRandomLevel();

            if (Level == null) {
                Debug.LogError($"{name}: Failed to select a LevelData.");
                return;
            }

            if (Level.Enemies == null || Level.Enemies.Count == 0) {
                Debug.LogError($"{name}: Level '{Level.name}' contains no enemies.");
                return;
            }

            enemy = Level.Enemies[Random.Range(0, Level.Enemies.Count)];

            if (enemy == null) {
                Debug.LogError($"{name}: Level '{Level.name}' selected a null PawnData.");
                return;
            }

            if (enemy.Pawn == null) {
                Debug.LogError($"{name}: PawnData '{enemy.name}' has no Pawn prefab.");
                return;
            }

            CombatEncounter combatEncounter = ScriptableObject.CreateInstance<CombatEncounter>();
            combatEncounter.EncounterTitle = Level.LevelName;
            combatEncounter.Background = Level.BackgroundPrefab;
            combatEncounter.Enemies = new List<PawnData> {
                enemy
            };

            Encounter = combatEncounter;

            Pawn enemyPreview = Instantiate(enemy.Pawn, EnemyContainer);
            enemyPreview.transform.localPosition = Vector3.zero;
            enemyPreview.transform.localRotation = Quaternion.identity;
            enemyPreview.Character.SetDirection(Vector2.down);
        }

        public async UniTask PassedTile() {}

        public async UniTask StoppedOnTile() {
            await UIEncounterWindow.Instance.LoadEncounter(Encounter);
        }

        private void OnValidate() {
            if (Encounter != null) {}
        }
    }
}