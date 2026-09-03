using _Rogues_Path._Game;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace _Rogues_Path.World.Encounters {
    [CreateAssetMenu(
        fileName = "New Traversal Encounter",
        menuName = Game.Name + "/Data/TraversalEncounterData")]
    public class TraversalEncounter : EncounterData {
        public override bool TriggersWhenStoppedOnTile => false;

        public string ButtonText = "Continue";
        public Vector3 WorldVisualScale = Vector3.one * 0.10f;

        public override Transform Initialize(Transform encounterContainer) {
            Transform visual = base.Initialize(encounterContainer);

            if (visual != null)
                visual.localScale = WorldVisualScale;

            return visual;
        }

        public override async UniTask HandleEncounter(
            Transform windowContent,
            Transform bottomBar,
            Button buttonPrefab) {

            await WaitForConfirmation(bottomBar, buttonPrefab, ButtonText);
        }
    }
}
