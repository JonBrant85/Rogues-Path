using System;
using _Rogues_Path.Pawns;
using _Rogues_Path.Utilities;
using DuloGames.UI;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Rogues_Path.UI.ActionBar {
    public class UIActionBar : Singleton<UIActionBar> {
        [FoldoutGroup("References"), SerializeField] private Pawn Player;
        [FoldoutGroup("References"), SerializeField] private UIProgressBar ActionTimeRemaining;

        public void SetPlayer(Pawn player) => Player = player;

        private void Update() {
            if (Player == null) return;

            ActionTimeRemaining.fillAmount = Player.Brain.TimeUntilAction / Player.Brain.ActionDelay;
            ActionTimeRemaining.UpdateBarFill();
        }
    }
}