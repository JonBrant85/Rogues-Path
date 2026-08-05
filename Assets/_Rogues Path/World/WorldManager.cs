using System;
using System.Collections.Generic;
using System.Linq;
using _Rogues_Path._Game;
using _Rogues_Path.Utilities;
using Assets.HeroEditor4D.Common.Scripts.Enums;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Rogues_Path.World {
    public class WorldManager : Singleton<WorldManager> {
        [FoldoutGroup("Settings"), SerializeField] private float MovementJump = 1;
        [FoldoutGroup("Settings"), SerializeField] private float MovementDuration = 1f;
        [FoldoutGroup("References"), SerializeField] private WorldTile StartingTile;
        [FoldoutGroup("Debug"), SerializeField] private FourDPawn PlayerPawn;
        [FoldoutGroup("Debug"), SerializeField] private List<WorldTile> Tiles = new();
        [FoldoutGroup("Debug"), SerializeField] private WorldTile currentTile;

        private void Awake() {
            PlayerPawn = Instantiate(Game.Instance.PlayerData.FourDPawn, StartingTile.PawnContainer);
            currentTile = StartingTile;
            Tiles = FindObjectsByType<WorldTile>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).ToList();
        }


        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                MoveToNextTile();
            }
        }


        private void MoveToNextTile() {
            Vector3 movementDirection = currentTile.NextTile.transform.position - currentTile.transform.position;
            PlayerPawn.xDirection = movementDirection.x;
            PlayerPawn.yDirection = movementDirection.z;
            PlayerPawn.animationManager.SetState(CharacterState.Jump);
            
            PlayerPawn.transform
                .DOJump(currentTile.NextTile.PawnContainer.transform.position, MovementJump, 1, MovementDuration, false)
                .OnComplete(
                    () => {
                        currentTile = currentTile.NextTile;
                        PlayerPawn.transform.SetParent(currentTile.PawnContainer);
                        PlayerPawn.animationManager.SetState(CharacterState.Idle);
                    });
        }
    }
}