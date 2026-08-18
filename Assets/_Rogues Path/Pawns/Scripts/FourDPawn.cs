using Assets.HeroEditor4D.Common.Scripts.CharacterScripts;
using Assets.HeroEditor4D.Common.Scripts.Enums;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Rogues_Path.Pawns.Scripts {
    public partial class Pawn : MonoBehaviour {
        public float xDirection, yDirection;
        [FoldoutGroup("References"), SerializeField] public AnimationManager animationManager;

        private void Start() {
            Character.SetDirection(Vector2.down);
            animationManager.SetState(CharacterState.Idle);
        }

        private void Update() {
            // Billboarding
            Vector3 targetPosition = Camera.main!.transform.position;
            targetPosition.y = transform.position.y;
            transform.LookAt(targetPosition);

            (float x, float y) direction = (xDirection, yDirection);

            switch (direction) {
                default:
                    animationManager.SetState(CharacterState.Idle);
                    break;
                case (< 0, 0):
                    Character.SetDirection(Vector2.right);
                    break;
                case (> 0, 0):
                    Character.SetDirection(Vector2.left);
                    break;
                case (0, <0):
                    Character.SetDirection(Vector2.down);
                    break;
                case (0, >0):
                    Character.SetDirection(Vector2.up);
                    break;
            }
        }
    }
}