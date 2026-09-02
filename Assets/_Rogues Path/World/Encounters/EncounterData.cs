using _Rogues_Path._Game;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace _Rogues_Path.World.Encounters {
    public class EncounterData : ScriptableObject {
        public string EncounterTitle = "Default encounter title";
        [TextArea] public string EncounterDescription;
        public Sprite WorldIndicatorSprite;
        public GameObject WorldVisualPrefab;

        public virtual void Initialize(Transform encounterContainer) {
            if (WorldVisualPrefab == null)
                return;

            GameObject worldVisual = Instantiate(WorldVisualPrefab, encounterContainer);
            worldVisual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        public virtual UniTask HandleEncounter(Transform windowContent, Transform bottomBar, Button buttonPrefab) =>
            UniTask.CompletedTask;

        protected static async UniTask<bool> WaitForConfirmation(Transform bottomBar, Button buttonPrefab, string buttonText) {
            if (bottomBar == null || buttonPrefab == null) {
                Debug.LogError("Encounter confirmation UI is not configured.");
                return false;
            }

            Button confirmationButton = Instantiate(buttonPrefab, bottomBar);
            Text label = confirmationButton.GetComponentInChildren<Text>();

            if (label != null)
                label.text = buttonText;

            bool clicked = false;
            confirmationButton.onClick.AddListener(Confirm);
            confirmationButton.gameObject.SetActive(true);

            await UniTask.WaitUntil(() => clicked);

            confirmationButton.onClick.RemoveListener(Confirm);
            Destroy(confirmationButton.gameObject);

            return true;

            void Confirm() {
                confirmationButton.interactable = false;
                clicked = true;
            }
        }
    }
}
