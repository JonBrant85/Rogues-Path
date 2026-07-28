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


        [Button]
        public static void Show() {
            Instance.transform.DOMove(Instance.ShownPosition, Instance.TransitionDuration);
        }

        [Button]
        public static void Hide() {
            Instance.transform.DOMove(Instance.HiddenPosition, Instance.TransitionDuration);
        }
    }
}