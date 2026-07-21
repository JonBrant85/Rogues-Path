using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Rogues_Path._Game {
    public class GameStateTriggerer : MonoBehaviour {
        enum TriggerModes {
            Awake,
            OnEnable,
            Delay,
            Button
        }

        [SerializeField] private TriggerModes TriggerMode;
        [SerializeField, ShowIf(nameof(TriggerMode), TriggerModes.Delay)] private float SecondsDelay = 1;
        [SerializeField] private Trigger Trigger;

        public void SendTrigger() {
            Game.Instance.FireTrigger(Trigger);
        }

        private async UniTaskVoid SendTriggerDelayed() {
            await UniTask.Delay((int)(SecondsDelay * 1000));
            SendTrigger();
        }

        private void Awake() {
            switch (TriggerMode) {
                case TriggerModes.Awake:
                    SendTrigger();
                    break;
                case TriggerModes.Delay:
                    SendTriggerDelayed()
                        .Forget();
                    break;
            }
        }

        private void OnEnable() {
            if (TriggerMode == TriggerModes.OnEnable) {
                SendTrigger();
            }
        }
    }
}