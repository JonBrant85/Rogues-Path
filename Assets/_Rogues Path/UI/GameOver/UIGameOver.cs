using _Rogues_Path._Game;
using UnityEngine;
using UnityEngine.UI;

namespace _Rogues_Path.UI.GameOver {
    public sealed class UIGameOver : MonoBehaviour {
        [SerializeField] private Button MainMenuButton;
        [SerializeField] private Button QuitButton;
        [SerializeField] private Text JourneyValues;
        [SerializeField] private Text CombatValues;

        private void Awake() {
            MainMenuButton.onClick.AddListener(ReturnToMainMenu);
            QuitButton.onClick.AddListener(QuitGame);
        }

        private void OnEnable() {
            // Looking up an existing Game keeps opening this scene alone from creating a run.
            Game game = FindObjectOfType<Game>();
            if (game == null) {
                JourneyValues.text = "0\n0\n0\n0\n0";
                CombatValues.text = "0\n0\n0\n0";
                return;
            }

            JourneyValues.text = $"{game.TilesTraveled:N0}\n"
                + $"{game.CombatsCleared:N0}\n"
                + $"{game.CompletedWorldTraversals:N0}\n"
                + $"{game.TreasuresClaimed:N0}\n"
                + $"{game.OrbsUsed:N0}";

            CombatValues.text = $"{game.DamageDealt:#,0.#}\n"
                + $"{game.DamageTaken:#,0.#}\n"
                + $"{game.HealthRestored:#,0.#}\n"
                + $"{game.BiggestSingleHit:#,0.#}";
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
