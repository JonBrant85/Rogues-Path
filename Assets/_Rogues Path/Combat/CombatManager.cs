using System;
using System.Collections;
using _Rogues_Path._Game;
using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.Pawns;
using _Rogues_Path.Pawns.Brains;
using _Rogues_Path.UI;
using _Rogues_Path.UI.ActionBar;
using _Rogues_Path.UI.CharacterScreen;
using _Rogues_Path.UI.MenuBar;
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

        private void Start() {
            UIActionBar.Show();
            UIMenuBar.Show();
        }

        private void OnEnable() {
            EventBus.SubscribeTo<CombatEncounterStarted>(CombatStartedEventHandler);
        }

        private void OnDisable() {
            EventBus.UnsubscribeFrom<CombatEncounterStarted>(CombatStartedEventHandler);
        }

        private void CombatStartedEventHandler(ref CombatEncounterStarted eventData) {
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

            foreach (var kvp in Game.Instance.PlayerEquipment) {
                if (EquipmentDatabase.TryGetByID(kvp.Value, out EquipmentBase equipment)) {
                    Player.TryEquip(equipment, false);
                }
            }
        }

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