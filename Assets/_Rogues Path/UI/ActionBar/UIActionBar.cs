using System;
using System.Linq;
using _Rogues_Path.Pawns;
using _Rogues_Path.Utilities;
using DuloGames.UI;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Rogues_Path.UI.ActionBar {
    public class UIActionBar : Singleton<UIActionBar> {
        public const int NumberOfSlots = 10;
        [FoldoutGroup("References"), SerializeField, HideInInspector] private Pawn Player;
        [FoldoutGroup("References"), SerializeField] private UIProgressBar ActionTimeRemaining;
        [FoldoutGroup("References"), SerializeField] private Transform SlotsContainer;
        [FoldoutGroup("References"), SerializeField] private UISpellSlot[] Slots = new UISpellSlot[NumberOfSlots];

        private void Update() {
            if (Player == null) return;

            ActionTimeRemaining.fillAmount = Player.Brain.TimeUntilAction / Player.Brain.ActionDelay;
            ActionTimeRemaining.UpdateBarFill();
        }

        public void SetPlayer(Pawn player) {
            Player = player;

            for (int i = 0; i < NumberOfSlots && i < Player.Brain.KnownSpells.Count; i++) {
                Slots[i]
                    .Assign(Player.Brain.KnownSpells[i]);
            }
        }

        public void TriggerSpellCooldown(UISpellInfo spell) {
            var slot = Slots.FirstOrDefault(spellSlot => spellSlot.GetSpellInfo() == spell);

            slot!.cooldownComponent.StartCooldown(spell.ID, spell.Cooldown);
        }

        public UISpellInfo GetFirstSpellOffCooldown() {
            for (int i = 0; i < Slots.Length; i++) {
                if (!Slots[i]
                    .cooldownComponent.IsOnCooldown) {
                    return Slots[i]
                        .GetSpellInfo();
                }
            }

            return null;
        }
    }
}