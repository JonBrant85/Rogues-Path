using System.Collections;
using _Rogues_Path._Game;
using _Rogues_Path.Pawns;
using _Rogues_Path.UI;
using _Rogues_Path.UI.ActionBar;
using _Rogues_Path.UI.CharacterScreen;
using _Rogues_Path.Utilities;
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

        private void Update() {
            Enemy.Brain.TimeUntilAction -= Time.deltaTime;
            Player.Brain.TimeUntilAction -= Time.deltaTime;

            if (Game.CommandInvoker.QueueCount == 0 && Player.Brain.TimeUntilAction <= 0) {
                Player.Brain.HandleTurn().Forget();
                Player.Brain.TimeUntilAction = Player.Brain.ActionDelay;
            }


            if (Game.CommandInvoker.QueueCount == 0 && Enemy.Brain.TimeUntilAction <= 0) {
                Enemy.Brain.HandleTurn().Forget();
                Enemy.Brain.TimeUntilAction = Enemy.Brain.ActionDelay;
            }
        }
    }
}