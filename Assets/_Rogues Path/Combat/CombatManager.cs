using System;
using System.Collections;
using _Rogues_Path._Game;
using _Rogues_Path.Pawns;
using _Rogues_Path.Pawns.Brains;
using _Rogues_Path.UI;
using _Rogues_Path.UI.ActionBar;
using _Rogues_Path.UI.CharacterScreen;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Rogues_Path.Combat {
    public class CombatManager : Singleton<CombatManager> {
        public Pawn Player;
        public Pawn Enemy;

        [FoldoutGroup("References"), SerializeField] private Transform BackgroundContainer;
        [FoldoutGroup("References"), SerializeField] private Transform PlayerContainer;
        [FoldoutGroup("References"), SerializeField] private Transform EnemyContainer;

        private void OnEnable() {
            EventBus.SubscribeTo<CombatEncounterStarted>(CombatStartedEventHandler);
        }

        private void OnDisable() {
            EventBus.UnsubscribeFrom<CombatEncounterStarted>(CombatStartedEventHandler);
        }

        private void CombatStartedEventHandler(ref CombatEncounterStarted eventData) {
            Debug.Log($"Heard combat started event!");
            var randomEnemy = eventData.Enemies.GetRandomElement();
            // Instantiate background, player, enemies
            Instantiate(eventData.BackgroundPrefab, BackgroundContainer);
            Player = Instantiate(Game.Instance.PlayerData.TwoDPawn, PlayerContainer);
            Enemy = Instantiate(randomEnemy.TwoDPawn, EnemyContainer);
            
            // Reset action timers
            Enemy.Brain.TimeUntilAction = Enemy.Brain.ActionDelay;
            Player.Brain.TimeUntilAction = Player.Brain.ActionDelay;
            
            UIActionBar.Instance.SetPlayer(Player);
            UISpellBook.Instance.SetPlayer(Player);
            UICharacterScreen.Instance.SetPlayer(Game.Instance.PlayerData);

            foreach (var equipment in Player.CurrentEquipment.Values) {
                Player.Character.Equip(equipment.Sprite, equipment.EquipType, equipment.SpriteColor);
            }
        }

        /*
        private void Start() {
            var levelData = Game.Instance.LevelData;
            var randomEnemy = levelData.Enemies.GetRandomElement();

            // Instantiate background, player, enemies
            Instantiate(levelData.BackgroundPrefab, BackgroundContainer);
            Player = Instantiate(Game.Instance.PlayerData.TwoDPawn, PlayerContainer);
            Enemy = Instantiate(randomEnemy, EnemyContainer);

            // Reset action timers
            Enemy.Brain.TimeUntilAction = Enemy.Brain.ActionDelay;
            Player.Brain.TimeUntilAction = Player.Brain.ActionDelay;

            UIActionBar.Instance.SetPlayer(Player);
            UISpellBook.Instance.SetPlayer(Player);
            UICharacterScreen.Instance.SetPlayer(Game.Instance.PlayerData);

            foreach (var equipment in Player.CurrentEquipment.Values) {
                Player.Character.Equip(equipment.Sprite, equipment.EquipType, equipment.SpriteColor);
            }
        }
        */

        private void Update() {
            if (Player == null || Enemy == null) return;

            HandleBrains(Player.Brain, Enemy.Brain);

            void HandleBrains(PawnBrain playerBrain, PawnBrain enemyBrain) {
                if (playerBrain == null || enemyBrain == null) return;

                playerBrain.TimeUntilAction -= Time.deltaTime;
                enemyBrain.TimeUntilAction -= Time.deltaTime;

                if (Game.CommandInvoker.QueueCount == 0 && playerBrain.TimeUntilAction <= 0) {
                    playerBrain.HandleTurn().Forget();
                    playerBrain.TimeUntilAction = playerBrain.ActionDelay;
                }


                if (Game.CommandInvoker.QueueCount == 0 && enemyBrain.TimeUntilAction <= 0) {
                    enemyBrain.HandleTurn().Forget();
                    enemyBrain.TimeUntilAction = enemyBrain.ActionDelay;
                }
            }
        }
    }
}