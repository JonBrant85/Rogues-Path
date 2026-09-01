using _Rogues_Path._Game;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace _Rogues_Path.World.Encounters {
    [CreateAssetMenu(fileName = "New Rest Encounter", menuName = Game.Name + "/Data/RestEncounterData")]
    public class RestEncounter : EncounterData {
        [Range(0f, 1f)] public float RestoredHealthPercentage = 0.3f;
        public string ButtonText = "Rest";

        public override async UniTask HandleEncounter(Transform windowContent, Transform bottomBar, Button buttonPrefab) {
            if (!await WaitForConfirmation(bottomBar, buttonPrefab, ButtonText))
                return;

            float restoredHealth = WorldManager.Instance.HealPlayer(RestoredHealthPercentage);
            Debug.Log($"Rested for {restoredHealth:0.#} health. Current health={Game.Instance.PlayerCurrentHealth:0.#}.");
        }
    }
}
