using _Rogues_Path._Game;
using UnityEngine;
using UnityEngine.UI;

namespace _Rogues_Path.UI.GameOver {
    public sealed class UIGameOver : MonoBehaviour {
        [SerializeField] private Button MainMenuButton;
        [SerializeField] private Button QuitButton;
        [SerializeField] private Text TilesTraveledValue;
        [SerializeField] private Text CombatsClearedValue;
        [SerializeField] private Text WorldsCompletedValue;
        [SerializeField] private Text TreasuresClaimedValue;
        [SerializeField] private Text OrbsUsedValue;
        [SerializeField] private Text DamageDealtValue;
        [SerializeField] private Text DamageTakenValue;
        [SerializeField] private Text HealthRestoredValue;
        [SerializeField] private Text BiggestSingleHitValue;

        private void Awake() {
            MainMenuButton.onClick.AddListener(ReturnToMainMenu);
            QuitButton.onClick.AddListener(QuitGame);
        }

        private void OnEnable() {
            // Looking up an existing Game keeps opening this scene alone from creating a run.
            Game game = FindObjectOfType<Game>();
            TilesTraveledValue.text = (game != null ? game.TilesTraveled : 0).ToString("N0");
            CombatsClearedValue.text = (game != null ? game.CombatsCleared : 0).ToString("N0");
            WorldsCompletedValue.text = (game != null ? game.CompletedWorldTraversals : 0).ToString("N0");
            TreasuresClaimedValue.text = (game != null ? game.TreasuresClaimed : 0).ToString("N0");
            OrbsUsedValue.text = (game != null ? game.OrbsUsed : 0).ToString("N0");
            DamageDealtValue.text = (game != null ? game.DamageDealt : 0).ToString("#,0.#");
            DamageTakenValue.text = (game != null ? game.DamageTaken : 0).ToString("#,0.#");
            HealthRestoredValue.text = (game != null ? game.HealthRestored : 0).ToString("#,0.#");
            BiggestSingleHitValue.text = (game != null ? game.BiggestSingleHit : 0).ToString("#,0.#");
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
