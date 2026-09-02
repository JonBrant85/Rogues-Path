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
        [FoldoutGroup("References"), SerializeField] private Text bodyText;
        [FoldoutGroup("References"), SerializeField] private UIPositionMover BottomBarMover;
        [FoldoutGroup("References"), SerializeField] private UIWindow Window;
        [FoldoutGroup("References"), SerializeField] private Transform WindowContent;
        [FoldoutGroup("References"), SerializeField] private Transform BottomBar;
        [FoldoutGroup("References"), SerializeField] private Button ButtonPrefab;
        

        public async UniTask LoadEncounter(EncounterData data) {
            if (data == null) {
                Debug.LogError("Cannot load a null encounter.");
                return;
            }

            if (!data.UsesEncounterWindow) {
                await data.HandleEncounter(WindowContent, BottomBar, ButtonPrefab);
                return;
            }

            titleText.text = data.EncounterTitle;

            if (bodyText != null)
                bodyText.text = data.EncounterDescription;

            Show();
            await data.HandleEncounter(WindowContent, BottomBar, ButtonPrefab);

            if (this == null)
                return;

            Hide();
        }

        public void Show() {
            Window.Show();
            BottomBarMover.Hide();
        }

        public void Hide() {
            Window.Hide();
            BottomBarMover.Show();
        }
    }
}
