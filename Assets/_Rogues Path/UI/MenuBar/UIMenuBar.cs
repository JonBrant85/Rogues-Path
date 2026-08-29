using System;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Rogues_Path.UI.MenuBar {
    public class UIMenuBar : MonoBehaviour {
        public float TransitionDuration = 0.5f;
        public Vector2 ShownPosition;
        public Vector2 HiddenPosition;

        private RectTransform rectTransform;

        private void Awake() {
            rectTransform = GetComponent<RectTransform>();
        }

        private void OnEnable() {
            EventBus.SubscribeTo<CombatEncounterStarted>(CombatEncounterStartedEventHandler);
            EventBus.SubscribeTo<CombatEncounterEnded>(CombatEncounterEndedEventHandler);
        }

        private void OnDisable() {
            EventBus.UnsubscribeFrom<CombatEncounterStarted>(CombatEncounterStartedEventHandler);
            EventBus.UnsubscribeFrom<CombatEncounterEnded>(CombatEncounterEndedEventHandler);
        }

        private void CombatEncounterStartedEventHandler(ref CombatEncounterStarted eventData) {
            Show();
        }

        private void CombatEncounterEndedEventHandler(ref CombatEncounterEnded eventData) {
            Hide();
        }

        [Button]
        public void Show() {
            rectTransform.DOKill();

            rectTransform.DOAnchorPos(ShownPosition, TransitionDuration);
        }

        [Button]
        public void Hide() {
            rectTransform.DOKill();

            rectTransform.DOAnchorPos(HiddenPosition, TransitionDuration);
        }
    }
}