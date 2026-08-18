using Assets.HeroEditor4D.Common.Scripts.CharacterScripts;
using Assets.HeroEditor4D.Common.Scripts.Enums;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Rogues_Path.Pawns.Scripts {
    public partial class Pawn : MonoBehaviour {
        [FoldoutGroup("References"), SerializeField] public AnimationManager animationManager;

        private void Update() {
            // Billboarding
            Vector3 targetPosition = Camera.main!.transform.position;
            targetPosition.y = transform.position.y;
            transform.LookAt(targetPosition);
        }
    }
}