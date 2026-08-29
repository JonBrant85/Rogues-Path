using Assets.HeroEditor4D.Common.Scripts.CharacterScripts;
using Assets.HeroEditor4D.Common.Scripts.Enums;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Rogues_Path.Pawns.Scripts {
    public partial class Pawn : MonoBehaviour {
        [FoldoutGroup("References"), SerializeField] public AnimationManager animationManager;

        private Camera billboardCamera;

        public void SetBillboardCamera(Camera cam) {
            billboardCamera = cam;
        }
        
        private void Update() {
            Camera cam =
                billboardCamera != null
                    ? billboardCamera
                    : Camera.main;

            if (cam == null)
                return;

            Vector3 targetPosition =
                cam.transform.position;

            targetPosition.y =
                transform.position.y;

            transform.LookAt(targetPosition);
        }
    }
}