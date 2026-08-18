using System;
using System.Linq;
using _Rogues_Path.Pawns;
using _Rogues_Path.Pawns.Scripts;
using DG.Tweening;
using DuloGames.UI;
using DuloGames.UI.Tweens;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

namespace _Rogues_Path.UI.ActionBar {
    public class UIActionBar : Utilities.Singleton<UIActionBar> {
        public const int NumberOfSlots = 10;
        [FoldoutGroup("References"), SerializeField] private Pawn Player;
        [FoldoutGroup("References"), SerializeField] private UIProgressBar ActionTimeRemaining;
        [FoldoutGroup("References"), SerializeField] private UISpellSlot[] Slots = new UISpellSlot[NumberOfSlots];

        private void Update() {
            if (Player == null) return;

            ActionTimeRemaining.fillAmount = Player.Brain.TimeUntilAction / Player.Brain.ActionDelay;
            ActionTimeRemaining.UpdateBarFill();
        }

        public void SetPlayer(Pawn player) {
            Player = player;

            for (int i = 0; i < NumberOfSlots && i < Player.Brain.PreparedSpells.Count; i++) {
                Debug.Assert(Slots[i] != null);

                if (Slots[i] == null) {
                    Debug.Log($"{gameObject.name}");
                }
                Debug.Assert(Player != null);
                Debug.Assert(Player.Brain != null);
                Debug.Assert(Player.Brain.PreparedSpells != null);
                Slots[i].Assign(Player.Brain.PreparedSpells[i]);
            }
        }

        public void TriggerSpellCooldown(UISpellInfo spell) {
            var slot = Slots.FirstOrDefault(spellSlot => spellSlot.GetSpellInfo() == spell);

            slot!.cooldownComponent.StartCooldown(spell.ID, spell.Cooldown);
        }

        public UISpellInfo GetFirstSpellOffCooldown() {
            for (int i = 0; i < Slots.Length; i++) {
                if (!Slots[i].cooldownComponent.IsOnCooldown) {
                    return Slots[i].GetSpellInfo();
                }
            }

            return null;
        }

        [Button]
        public static void Show() {
            Instance.transform.DOMoveY(0, 0.5f);
        }

        [Button]
        public static void Hide() {
            Instance.transform.DOMoveY(-110, 0.5f);
        }
    }
}