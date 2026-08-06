using _Rogues_Path.UI;
using _Rogues_Path.Utilities;
using Cysharp.Threading.Tasks;
using DuloGames.UI;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Rogues_Path.World.Encounters {
    public class UIEncounterWindow : Singleton<UIEncounterWindow> {
        [FoldoutGroup("References"), SerializeField] private Text titleText;
        [FoldoutGroup("References"), SerializeField] private UIPositionMover BottomBarMover;
        [FoldoutGroup("References"), SerializeField] private UIWindow Window;
        [FoldoutGroup("References"), SerializeField] private Transform WindowContent;
        [FoldoutGroup("References"), SerializeField] private Transform BottomBar;
        [FoldoutGroup("References"), SerializeField] private Button ButtonPrefab;
        

        public async UniTask LoadEncounter(EncounterData data) {
            titleText.text = data.EncounterTitle;
            Show();
            await data.HandleEncounter(WindowContent, BottomBar, ButtonPrefab);
        }

        private void Show() {
            Window.Show();
            BottomBarMover.Hide();
        }

        private void Hide() {
            Window.Hide();
            BottomBarMover.Show();
        }
    }
}