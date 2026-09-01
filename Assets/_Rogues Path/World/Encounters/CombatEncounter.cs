using System.Collections.Generic;
using _Rogues_Path._Game;
using _Rogues_Path.Pawns;
using _Rogues_Path.Pawns.Scripts;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _Rogues_Path.World.Encounters {
    [CreateAssetMenu(fileName = "New Combat Encounter", menuName = Game.Name + "/Data/CombatEncounterData")]
    public class CombatEncounter : EncounterData {
        public GameObject Background;
        public List<PawnData> Enemies = new();

        public override void Initialize(Transform encounterContainer) {
            if (Enemies == null) {
                Debug.LogError($"Combat encounter '{name}' has a null enemy list.");
                Enemies = new List<PawnData>();
                return;
            }

            List<PawnData> validEnemies = Enemies.FindAll(enemy => enemy != null && enemy.Pawn != null);

            if (validEnemies.Count == 0) {
                Debug.LogError($"Combat encounter '{name}' contains no valid enemies.");
                Enemies.Clear();
                return;
            }

            PawnData selectedEnemy = validEnemies[Random.Range(0, validEnemies.Count)];

            Enemies = new List<PawnData> {
                selectedEnemy
            };

            Pawn enemyPreview = Instantiate(selectedEnemy.Pawn, encounterContainer);
            enemyPreview.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            enemyPreview.Character.SetDirection(Vector2.down);
        }

        public async override UniTask HandleEncounter(Transform windowContent, Transform bottomBar, Button buttonPrefab) {
            if (Enemies.Count == 0) {
                Debug.LogError($"Combat encounter '{name}' cannot start without an enemy.");
                return;
            }

            var combatStartedEvent = new CombatEncounterStarted {
                BackgroundPrefab = Background,
                Enemies = Enemies
            };

            Game.FireTrigger(Trigger.EnterCombat);
            await UniTask.Delay(2000);
            EventBus.Raise(combatStartedEvent);
        }
    }
}
