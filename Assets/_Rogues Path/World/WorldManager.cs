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
using UnityEngine.UI;
using Random = System.Random;

namespace _Rogues_Path.World {
    public class WorldManager : Singleton<WorldManager> {
        [FoldoutGroup("Settings/Movement"), SerializeField] private float MovementJump = 1;
        [FoldoutGroup("Settings/Movement"), SerializeField] private float MovementDuration = 1f;

        [FoldoutGroup("Settings"), SerializeField] private float DieDropHeight = 4f;
        [FoldoutGroup("Settings"), SerializeField] private float DieAngularVelocityMultiplier = 90f;
        [FoldoutGroup("Settings"), SerializeField] private float DieLifetime = 5f;
        [FoldoutGroup("Settings"), SerializeField] private float DieBufferCoefficient = 0.8f;
        [FoldoutGroup("Settings"), SerializeField] private MeshCollider DiePlaneCollider;

        [FoldoutGroup("References"), SerializeField] private WorldTile StartingTile;
        [FoldoutGroup("References"), SerializeField] private Die DiePrefab;
        [FoldoutGroup("References"), SerializeField] private Button MoveButton;

        [FoldoutGroup("Debug"), SerializeField] private FourDPawn PlayerPawn;
        [FoldoutGroup("Debug"), SerializeField] private WorldTile currentTile;

        private void Awake() {
            PlayerPawn = Instantiate(Game.Instance.PlayerData.FourDPawn, StartingTile.PawnContainer);
            currentTile = StartingTile;
        }


        /*
        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                MoveToNextTile();
            }

            if (Input.GetMouseButtonDown(1)) {
                RollDie().Forget();
            }
        }
        */

        public async void MoveButtonPressed() {
            MoveButton.interactable = false;
            var diceRolls = await RollDice(1);
            var total = diceRolls.Sum();

            for (int i = 0; i < total; i++) {
                await MoveToNextTile();
            }

            MoveButton.interactable = true;
        }

        private async UniTask MoveToNextTile() {
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
            await UniTask.Delay((int)MovementDuration * 1000);
        }

        private async UniTask<List<int>> RollDice(int numberOfDice) {
            // Keeping a list of RollDie tasks for a UniTask.WhenAll call
            List<UniTask<int>> diceRollTasks = new();

            // Collect tasks
            for (int i = 0; i < numberOfDice; i++) {
                UniTask<int> dieRollTask = RollDie();
                diceRollTasks.Add(dieRollTask);
            }

            // Execute and wait for all tasks to fill array
            var diceRollValues = await UniTask.WhenAll(diceRollTasks);

            // After all dice are finished rolling, display their values and return them
            for (int i = 0; i < diceRollValues.Length; i++) {
                Debug.Log($"Rolled a {diceRollValues[i]}");
            }

            return diceRollValues.ToList();
        }

        private async UniTask<int> RollDie() {
            // Plane the die drops on
            var planeExtents = DiePlaneCollider.bounds.extents;

            // Get a 'suitable' random position within planeExtents at dropHeight height
            var dropPosition = new Vector3(
                UnityEngine.Random.Range(-planeExtents.x, planeExtents.x) * DieBufferCoefficient,
                DieDropHeight,
                UnityEngine.Random.Range(-planeExtents.y, planeExtents.y) * DieBufferCoefficient);

            // Instantiate with random rotation
            var die = Instantiate(DiePrefab, dropPosition, UnityEngine.Random.rotation);

            // Give the die a random spin
            die.RigidBody.angularVelocity = UnityEngine.Random.rotation.eulerAngles * DieAngularVelocityMultiplier;

            // Wait until ReadDie isn't null, meaning it has stopped
            await UniTask.WaitUntil(() => die.ReadDie() != null);
            Destroy(die.gameObject, DieLifetime);
            return die.ReadDie()!.Value;
        }
    }
}