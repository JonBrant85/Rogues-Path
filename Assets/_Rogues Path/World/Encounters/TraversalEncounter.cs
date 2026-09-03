using _Rogues_Path._Game;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace _Rogues_Path.World.Encounters {
    [CreateAssetMenu(
        fileName = "New Traversal Encounter",
        menuName = Game.Name + "/Data/TraversalEncounterData")]
    public class TraversalEncounter : EncounterData {
        public string ButtonText = "Continue";

        public override async UniTask HandleEncounter(
            Transform windowContent,
            Transform bottomBar,
            Button buttonPrefab) {

            await WaitForConfirmation(bottomBar, buttonPrefab, ButtonText);
        }
    }
}
