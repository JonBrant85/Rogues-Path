using _Rogues_Path.Utilities;
using DuloGames.UI;
using UnityEngine;

namespace _Rogues_Path.UI.RewardsScreen {
    public class UIRewardsScreen : Singleton<UIRewardsScreen> {
        [SerializeField] private UIWindow Window;
        public UIBlackOverlay BlackOverlay;

        public static void Show() {
            Instance.Window.Show();
            Instance.BlackOverlay.Show();
        }

        public static void Hide() {
            Instance.Window.Hide();
            Instance.BlackOverlay.Hide();
        }
    }
}