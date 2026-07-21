using System;
using System.ComponentModel;
using _Rogues_Path._Game;
using _Rogues_Path.Pawns;
using _Rogues_Path.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Rogues_Path.Combat {
    public class CombatManager : Singleton<CombatManager> {
        public Pawn Player;
        public Pawn Enemy;
        
        [FoldoutGroup("References"), SerializeField] private Transform BackgroundContainer;
        [FoldoutGroup("References"), SerializeField] private Transform PlayerContainer;
        [FoldoutGroup("References"), SerializeField] private Transform EnemyContainer;


        private void Awake() {
            var levelData = Game.Instance.CurrentLevel;
            var randomEnemy = levelData.Enemies.GetRandomElement();

            Instantiate(levelData.BackgroundPrefab, BackgroundContainer);
            Instantiate(Game.Instance.CurrentCharacter, PlayerContainer);
            
        }
    }
}