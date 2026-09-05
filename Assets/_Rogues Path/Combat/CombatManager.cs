using System;
using System.Collections;
using _Rogues_Path._Game;
using _Rogues_Path.Crafting;
using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.Levels;
using _Rogues_Path.Pawns;
using _Rogues_Path.Pawns.Scripts;
using _Rogues_Path.Pawns.Scripts.Brains;
using _Rogues_Path.UI;
using _Rogues_Path.UI.ActionBar;
using _Rogues_Path.UI.CharacterScreen;
using _Rogues_Path.UI.MenuBar;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using _Rogues_Path.World;
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
        [FoldoutGroup("References"), SerializeField] private EquipmentModifierDatabase ModifierDatabase;


        private void OnEnable() {
            EventBus.SubscribeTo<CombatEncounterStarted>(CombatStartedEventHandler);
            EventBus.SubscribeTo<EquipmentEquippedEvent>(EquipmentEquippedHandler);
            EventBus.SubscribeTo<EquipmentUnequippedEvent>(EquipmentUnequippedEventHandler);
        }

        private void OnDisable() {
            EventBus.UnsubscribeFrom<CombatEncounterStarted>(CombatStartedEventHandler);
            EventBus.UnsubscribeFrom<EquipmentEquippedEvent>(EquipmentEquippedHandler);
            EventBus.UnsubscribeFrom<EquipmentUnequippedEvent>(EquipmentUnequippedEventHandler);
        }

        private void CombatStartedEventHandler(ref CombatEncounterStarted eventData) {
            var randomEnemy = eventData.Enemies.GetRandomElement();
            // Instantiate background, player, enemies
            if (eventData.BackgroundPrefab != null) Instantiate(eventData.BackgroundPrefab, BackgroundContainer);
            Player = Instantiate(Game.Instance.PlayerData.Pawn, PlayerContainer);
            Enemy = Instantiate(randomEnemy.Pawn, EnemyContainer);

            if (!EnemyTraversalScaler.TryApply(
                    Enemy,
                    Game.Instance.CompletedWorldTraversals,
                    WorldProgressionSettings.Instance)) {

                Debug.LogError($"Failed to scale enemy {Enemy.CharacterName}.");
            }

            // Setup spells
            Player.Brain.KnownSpells.Clear();
            Player.Brain.KnownSpells.AddRange(Game.Instance.PlayerData.ClassSpells);
            Player.Brain.PreparedSpells.Clear();
            Player.Brain.PreparedSpells.AddRange(Game.Instance.PlayerPreparedSpells);
            Enemy.Brain.KnownSpells.Clear();
            Enemy.Brain.KnownSpells.AddRange(randomEnemy.ClassSpells);

            // Face Player/Enemies the correct direction
            Player.Character.SetDirection(Vector2.right);
            Enemy.Character.SetDirection(Vector2.left);

            // Reset action timers
            Enemy.Brain.TimeUntilAction = Enemy.Brain.ActionDelay;
            Player.Brain.TimeUntilAction = Player.Brain.ActionDelay;

            UIActionBar.Instance.SetPlayer(Player);
            UISpellBook.Instance.SetPlayer(Player);
            UICharacterScreen.Instance.SetPlayer(Game.Instance.PlayerData);

            Player.SyncInventoryFromGameState();

            foreach (var pair in Game.Instance.PlayerEquipment) {
                EquipmentInstanceData instanceData = pair.Value;

                if (!EquipmentDatabase.TryCreateInstance(instanceData, ModifierDatabase, out EquipmentBase liveEquipment, Player.transform)) {

                    Debug.LogError($"Failed to create equipment instance for ID " + $"{instanceData.EquipmentID}.");

                    continue;
                }

                if (!Player.TryEquip(liveEquipment, false)) {
                    Destroy(liveEquipment.gameObject);
                }
            }

            PlayerHealthState.Restore(Player);
            EventBus.Raise(new RunPawnsChanged { Player = Player, Enemy = Enemy });
        }

        private void EquipmentUnequippedEventHandler(ref EquipmentUnequippedEvent eventData) {

            if (Player == null)
                return;

            if (eventData.Owner == Player)
                return;

            if (!Player.CurrentEquipment.TryGetValue(eventData.EquipType, out EquipmentBase liveEquipment)) {

                return;
            }

            Player.TryRemoveEquipment(liveEquipment, false);
        }

        private void EquipmentEquippedHandler(ref EquipmentEquippedEvent eventData) {

            if (Player == null || eventData.Equipment == null || eventData.Owner == Player) {

                return;
            }

            EquipmentInstanceData instanceData = eventData.Equipment.InstanceData;

            if (instanceData == null)
                return;

            if (!EquipmentDatabase.TryCreateInstance(instanceData, ModifierDatabase, out EquipmentBase liveEquipment, Player.transform)) {

                return;
            }

            if (!Player.TryEquip(liveEquipment, false)) {

                Destroy(liveEquipment.gameObject);
            }
        }

        private void Update() {
            if (Player == null || Enemy == null) return;

            HandleBrains(Player.Brain, Enemy.Brain);

            void HandleBrains(PawnBrain playerBrain, PawnBrain enemyBrain) {
                if (playerBrain == null || enemyBrain == null) return;

                playerBrain.TimeUntilAction -= Time.deltaTime;
                enemyBrain.TimeUntilAction -= Time.deltaTime;

                if (!Game.CommandInvoker.IsBusy && playerBrain.TimeUntilAction <= 0) {
                    playerBrain.HandleTurn().Forget();
                    playerBrain.TimeUntilAction = playerBrain.ActionDelay;
                }


                if (!Game.CommandInvoker.IsBusy && enemyBrain.TimeUntilAction <= 0) {
                    enemyBrain.HandleTurn().Forget();
                    enemyBrain.TimeUntilAction = enemyBrain.ActionDelay;
                }
            }
        }
    }
}
