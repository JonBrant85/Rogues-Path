using System.Collections.Generic;
using System.Threading.Tasks;
using _Rogues_Path._Game;
using _Rogues_Path.Pawns;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace _Rogues_Path.World.Encounters {
    [CreateAssetMenu(fileName = "New Combat Encounter", menuName = Game.Name + "/Data/CombatEncounterData")]
    public class CombatEncounter: EncounterData {
        public List<PawnData> Enemies = new();

        public async override UniTask HandleEncounter(Transform windowContent, Transform bottomBar, Button buttonPrefab) {
            Game.FireTrigger(Trigger.EnterCombat);
        }
    }
}