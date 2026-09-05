using _Rogues_Path._Game;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace _Rogues_Path.World.Encounters {
    [CreateAssetMenu(fileName = "New Rest Encounter", menuName = Game.Name + "/Data/RestEncounterData")]
    public class RestEncounter : EncounterData {
        [Range(0f, 1f)] public float RestoredHealthPercentage = 0.3f;
        public string ButtonText = "Rest";
        public string ContinueButtonText = "Continue";
        [TextArea] public string RestoredResultText = "The fire's warmth settles into your bones. You recover {amount} health.";
        [TextArea] public string FullHealthResultText = "You rest beside the flames, but your strength is already whole.";

        public override async UniTask HandleEncounter(
            Transform windowContent,
            Transform bottomBar,
            Button buttonPrefab) {

            if (!await WaitForConfirmation(bottomBar, buttonPrefab, ButtonText))
                return;

            float restoredHealth = WorldManager.Instance.HealPlayer(RestoredHealthPercentage);
            ExtinguishCampfire();

            string resultText = restoredHealth > 0f
                ? RestoredResultText.Replace("{amount}", restoredHealth.ToString("0.#"))
                : FullHealthResultText;

            Debug.Log($"Rested for {restoredHealth:0.#} health. Current health={Game.Instance.PlayerCurrentHealth:0.#}.");

            if (UIEncounterWindow.Instance != null)
                await UIEncounterWindow.Instance.ShowResult(resultText, ContinueButtonText);
        }

        private void ExtinguishCampfire() {
            if (RuntimeWorldVisual == null) {
                Debug.LogWarning($"Rest encounter '{name}' has no runtime campfire visual to extinguish.");
                return;
            }

            ParticleSystem[] particleSystems = RuntimeWorldVisual.GetComponentsInChildren<ParticleSystem>(true);
            Light[] lights = RuntimeWorldVisual.GetComponentsInChildren<Light>(true);

            if (particleSystems.Length == 0 && lights.Length == 0)
                Debug.LogWarning($"Rest encounter '{name}' found no fire effects to extinguish.");

            foreach (ParticleSystem particleSystem in particleSystems)
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            foreach (Light light in lights)
                light.enabled = false;
        }
    }
}
