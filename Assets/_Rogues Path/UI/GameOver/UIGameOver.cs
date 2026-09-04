using _Rogues_Path._Game;
using UnityEngine;
using UnityEngine.UI;

namespace _Rogues_Path.UI.GameOver {
    public sealed class UIGameOver : MonoBehaviour {
        [SerializeField] private Button MainMenuButton;
        [SerializeField] private Button QuitButton;

        private void Awake() {
            MainMenuButton.onClick.AddListener(ReturnToMainMenu);
            QuitButton.onClick.AddListener(QuitGame);
        }

        private void OnDestroy() {
            MainMenuButton.onClick.RemoveListener(ReturnToMainMenu);
            QuitButton.onClick.RemoveListener(QuitGame);
        }

        private void ReturnToMainMenu() {
            MainMenuButton.interactable = false;
            Game.FireTrigger(Trigger.EnterMainMenu);
        }

        private static void QuitGame() {
            Application.Quit();
        }
    }
}
