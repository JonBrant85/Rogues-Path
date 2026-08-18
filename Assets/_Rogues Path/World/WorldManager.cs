using System;
using System.Collections.Generic;
using System.Linq;
using _Rogues_Path._Game;
using _Rogues_Path.Pawns.Scripts;
using _Rogues_Path.Utilities;
using Assets.HeroEditor4D.Common.Scripts.Enums;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Michsky.UI.MTP;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

namespace _Rogues_Path.World {
    public class WorldManager : Singleton<WorldManager> {
        [FoldoutGroup("Settings")]
        [FoldoutGroup("Settings/Movement"), SerializeField] private float MovementJump = 1;
        [FoldoutGroup("Settings/Movement"), SerializeField] private float MovementDuration = 1f;

        [FoldoutGroup("Settings/Dice"), SerializeField] private int DiceCount = 2;
        [FoldoutGroup("Settings/Dice"), SerializeField] private float DieDropHeight = 4f;
        [FoldoutGroup("Settings/Dice"), SerializeField] private float DieAngularVelocityMultiplier = 90f;
        [FoldoutGroup("Settings/Dice"), SerializeField] private float DieLifetime = 5f;
        [FoldoutGroup("Settings/Dice"), SerializeField] private float DieBufferCoefficient = 0.8f;
        [FoldoutGroup("Settings/Dice"), SerializeField] private MeshCollider DiePlaneCollider;

        [FoldoutGroup("References"), SerializeField] private WorldTile StartingTile;
        [FoldoutGroup("References"), SerializeField] private Die DiePrefab;
        [FoldoutGroup("References"), SerializeField] private Button MoveButton;
        [FoldoutGroup("References"), SerializeField] private StyleManager DiceRollAnnouncer;

        [FoldoutGroup("Debug"), SerializeField] private Pawn PlayerPawn;
        [FoldoutGroup("Debug"), SerializeField] private WorldTile currentTile;

        private void Awake() {

            PlayerPawn = Instantiate(Game.Instance.PlayerData.Pawn, StartingTile.PawnContainer);
            currentTile = StartingTile;
        }

        public void UIMoveButtonPressed() {
            RollDiceAndMove(DiceCount).Forget();
        }

        public async UniTask RollDiceAndMove(int numberOfDice) {
            // Make sure number of dice makes sense, disable button
            Debug.Assert(numberOfDice > 0, $"Can't roll {numberOfDice} dice.");
            MoveButton.interactable = false;

            // Roll the dice, await results then total their values
            var diceRolls = await RollDice(numberOfDice);
            var total = diceRolls.Sum();

            // Update Dice Roll Announcer
            DiceRollAnnouncer.textItems[0].text = "Rolls:";
            DiceRollAnnouncer.textItems[1].text = diceRolls.ToCommaDelimitedString();
            DiceRollAnnouncer.textItems[2].text = $"Total: {total}";
            DiceRollAnnouncer.Play();

            // Move 'total' tiles, waiting for Passed/StoppedOnTile on the way
            for (int i = 0; i < total; i++) {
                await MoveToNextTile();

                if (i + 1 < total) {
                    await currentTile.PassedTile();
                }
                else {
                    await currentTile.StoppedOnTile();
                }
            }
        }

        private async UniTask MoveToNextTile() {
            // ToDo: Setup Character facing during movement again. I broke it
            PlayerPawn.animationManager.SetState(CharacterState.Jump);
            
            Tween tween = PlayerPawn.transform.DOJump(currentTile.NextTile.PawnContainer.transform.position, MovementJump, 1, MovementDuration, false);
           
            currentTile = currentTile.NextTile;
            PlayerPawn.transform.SetParent(currentTile.PawnContainer);
            PlayerPawn.animationManager.SetState(CharacterState.Idle);
            await tween.AsyncWaitForCompletion();
        }

        private async UniTask<List<int>> RollDice(int numberOfDice) {
            // Keeping a list of RollDie tasks for a UniTask.WhenAll call
            List<UniTask<int>> diceRollTasks = new();

            // Collect tasks
            for (int i = 0; i < numberOfDice; i++) {
                UniTask<int> dieRollTask = RollDie();
                diceRollTasks.Add(dieRollTask);
            }

            // Execute and wait for all tasks to fill array and return it
            var diceRollValues = await UniTask.WhenAll(diceRollTasks);
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