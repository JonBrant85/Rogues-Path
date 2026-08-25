using System;
using _Rogues_Path.Utilities;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Rogues_Path.UI.MenuBar {
    public class UIMenuBar : Singleton<UIMenuBar> {
        public float TransitionDuration = 0.5f;
        public Vector3 ShownPosition;
        public Vector3 HiddenPosition;

        private Vector3 InitialPosition = Vector3.zero;

        private void Awake() {
            InitialPosition = transform.localPosition;
        }

        [Button]
        public static void Show() {
            Instance.transform.DOLocalMove(Instance.InitialPosition + Instance.ShownPosition, Instance.TransitionDuration);
        }

        [Button]
        public static void Hide() {
            //Instance.transform.DOLocalMove(Instance.InitialPosition + Instance.HiddenPosition, Instance.TransitionDuration);
        }
    }
}