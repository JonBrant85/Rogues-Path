using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Rogues_Path.UI {
    public class UIPositionMover: MonoBehaviour {
        public float Duration = 0.5f;
        public float ShownYPosition;
        public float HiddenYPosition;

        [Button]
        public void Show() {
            transform.DOMoveY(ShownYPosition, Duration);
        }

        [Button]
        public void Hide() {
            transform.DOMoveY(HiddenYPosition, Duration);
        }
    }
}