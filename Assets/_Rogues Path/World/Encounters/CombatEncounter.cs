using System.Collections.Generic;
using _Rogues_Path._Game;
using _Rogues_Path.Pawns;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace _Rogues_Path.World.Encounters {
    [CreateAssetMenu(fileName = "New Combat Encounter", menuName = Game.Name + "/Data/CombatEncounterData")]
    public class CombatEncounter : EncounterData {
        public GameObject Background;
        public List<PawnData> Enemies = new();

        public async override UniTask HandleEncounter(Transform windowContent, Transform bottomBar, Button buttonPrefab) {
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