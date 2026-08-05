using System;
using System.Collections.Generic;
using System.Linq;
using _Rogues_Path._Game;
using _Rogues_Path.Utilities;
using Assets.HeroEditor4D.Common.Scripts.Enums;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = System.Random;

namespace _Rogues_Path.World {
    public class WorldManager : Singleton<WorldManager> {
        [FoldoutGroup("Settings"), SerializeField] private float MovementJump = 1;
        [FoldoutGroup("Settings"), SerializeField] private float MovementDuration = 1f;

        [FoldoutGroup("Settings"), SerializeField] private float DieDropHeight = 4f;
        [FoldoutGroup("Settings"), SerializeField] private float DieAngularVelocityMultiplier = 90f;
        [FoldoutGroup("Settings"), SerializeField] private float DieLifetime = 5f;
        [FoldoutGroup("Settings"), SerializeField] private float DieBufferCoefficient = 0.8f;
        [FoldoutGroup("Settings"), SerializeField] private MeshCollider DiePlaneCollider;

        [FoldoutGroup("References"), SerializeField] private WorldTile StartingTile;
        [FoldoutGroup("References"), SerializeField] private Die DiePrefab;

        [FoldoutGroup("Debug"), SerializeField] private FourDPawn PlayerPawn;
        [FoldoutGroup("Debug"), SerializeField] private WorldTile currentTile;

        private void Awake() {
            PlayerPawn = Instantiate(Game.Instance.PlayerData.FourDPawn, StartingTile.PawnContainer);
            currentTile = StartingTile;
        }


        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                MoveToNextTile();
            }

            if (Input.GetMouseButtonDown(1)) {
                RollDie().Forget();
            }
        }


        private void MoveToNextTile() {
            Vector3 movementDirection = currentTile.NextTile.transform.position - currentTile.transform.position;
            PlayerPawn.xDirection = movementDirection.x;
            PlayerPawn.yDirection = movementDirection.z;
            PlayerPawn.animationManager.SetState(CharacterState.Jump);

            PlayerPawn.transform.DOJump(currentTile.NextTile.PawnContainer.transform.position, MovementJump, 1, MovementDuration, false)
                .OnComplete(
                    () => {
                        currentTile = currentTile.NextTile;
                        PlayerPawn.transform.SetParent(currentTile.PawnContainer);
                        PlayerPawn.animationManager.SetState(CharacterState.Idle);
                    });
        }

        private async UniTask<int> RollDie() {
            var planeExtents = DiePlaneCollider.bounds.extents;
            var dropPosition = new Vector3(
                UnityEngine.Random.Range(-planeExtents.x, planeExtents.x) * DieBufferCoefficient,
                DieDropHeight,
                UnityEngine.Random.Range(-planeExtents.y, planeExtents.y) * DieBufferCoefficient);

            var die = Instantiate(DiePrefab, dropPosition, UnityEngine.Random.rotation);

            // Give the die a random spin
            die.RigidBody.angularVelocity = UnityEngine.Random.rotation.eulerAngles * DieAngularVelocityMultiplier;

            // Wait until ReadDie isn't null, meaning it has stopped
            await UniTask.WaitUntil(() => die.ReadDie() != null);
            Debug.Log($"Die value: {die.ReadDie()!.Value}");
            Destroy(die.gameObject, DieLifetime);
            return die.ReadDie()!.Value;
        }
    }
}