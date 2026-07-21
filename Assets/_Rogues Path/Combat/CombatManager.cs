using System;
using System.ComponentModel;
using _Rogues_Path._Game;
using _Rogues_Path.Utilities;
using UnityEngine;

namespace _Rogues_Path.Combat {
    public class CombatManager: Singleton<CombatManager> {
        public Transform BackgroundContainer;
        public Transform PlayerContainer;
        public Transform EnemyContainer;

        
        private void Awake() {
            var levelData = Game.Instance.CurrentLevel;
            var randomEnemy = levelData.Enemies.GetRandomElement();
            
            Instantiate(levelData.BackgroundPrefab, BackgroundContainer);
            Instantiate(Game.Instance.CurrentCharacter, PlayerContainer);
            
        }
    }
}