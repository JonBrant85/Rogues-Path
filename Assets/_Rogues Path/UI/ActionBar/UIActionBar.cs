using System.Linq;
using _Rogues_Path._Game;
using _Rogues_Path.Pawns.Scripts;
using _Rogues_Path.Spells;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using DG.Tweening;
using DuloGames.UI;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Rogues_Path.UI.ActionBar {
    public class UIActionBar : Singleton<UIActionBar> {
        public const int NumberOfSlots = 10;
        public float TransitionDuration = 0.5f;
        public Vector2 ShownPosition;
        public Vector2 HiddenPosition;

        [FoldoutGroup("References"), SerializeField] private Pawn Player;
        [FoldoutGroup("References"), SerializeField] private UIProgressBar ActionTimeRemaining;
        [FoldoutGroup("References"), SerializeField] private UISpellSlot[] Slots = new UISpellSlot[NumberOfSlots];

        private RectTransform rectTransform;
        private const int EmptySpellSlot = -1;
        private bool spellOrderDirty;
        private bool restoringSpellOrder;

        private void Awake() {
            rectTransform = GetComponent<RectTransform>();
        }

        private void OnEnable() {
            EventBus.SubscribeTo<CombatEncounterStarted>(CombatEncounterStartedEventHandler);
            EventBus.SubscribeTo<CombatEncounterEnded>(CombatEncounterEndedEventHandler);

            foreach (UISpellSlot slot in Slots) {
                slot.onAssign.AddListener(SpellSlotChanged);
                slot.onUnassign.AddListener(SpellSlotChanged);
            }
        }

        private void OnDisable() {
            EventBus.UnsubscribeFrom<CombatEncounterStarted>(CombatEncounterStartedEventHandler);
            EventBus.UnsubscribeFrom<CombatEncounterEnded>(CombatEncounterEndedEventHandler);

            foreach (UISpellSlot slot in Slots) {
                slot.onAssign.RemoveListener(SpellSlotChanged);
                slot.onUnassign.RemoveListener(SpellSlotChanged);
            }
        }

        private void LateUpdate() {
            if (!spellOrderDirty)
                return;

            spellOrderDirty = false;
            SaveSpellOrder();
        }

        private void SpellSlotChanged(UISpellSlot slot) {
            if (restoringSpellOrder)
                return;

            spellOrderDirty = true;
        }

        private void SaveSpellOrder() {
            Game.Instance.ActionBarSpellOrder.Clear();

            foreach (UISpellSlot slot in Slots) {
                UISpellInfo spell = slot.GetSpellInfo();

                if (spell == null) {
                    Game.Instance.ActionBarSpellOrder.Add(EmptySpellSlot);
                    continue;
                }

                if (SpellDatabase.Instance.TryGetID(spell, out int id)) {
                    Game.Instance.ActionBarSpellOrder.Add(id);
                }
            }
        }

        private void Update() {
            if (Player == null) return;

            ActionTimeRemaining.fillAmount = Player.Brain.TimeUntilAction / Player.Brain.ActionDelay;
            ActionTimeRemaining.UpdateBarFill();
        }

        private void CombatEncounterStartedEventHandler(ref CombatEncounterStarted eventData) {
            Show();
        }

        private void CombatEncounterEndedEventHandler(ref CombatEncounterEnded eventData) {
            Hide();
        }

        public void SetPlayer(Pawn player) {
            Player = player;
            restoringSpellOrder = true;

            foreach (UISpellSlot slot in Slots) {
                slot.Unassign();
            }

            if (Game.Instance.ActionBarSpellOrder.Count != NumberOfSlots) {
                Game.Instance.ActionBarSpellOrder.Clear();

                for (int i = 0; i < NumberOfSlots; i++) {
                    if (i < Player.Brain.PreparedSpells.Count && SpellDatabase.Instance.TryGetID(Player.Brain.PreparedSpells[i], out int id)) {
                        Game.Instance.ActionBarSpellOrder.Add(id);
                    }
                    else {
                        Game.Instance.ActionBarSpellOrder.Add(EmptySpellSlot);
                    }
                }
            }

            for (int i = 0; i < NumberOfSlots; i++) {
                int spellID = Game.Instance.ActionBarSpellOrder[i];

                if (spellID == EmptySpellSlot)
                    continue;

                if (SpellDatabase.TryGetByID(spellID, out UISpellInfo spell)) {
                    Slots[i].Assign(spell);
                }
            }

            restoringSpellOrder = false;
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

        public void Show() {
            rectTransform.DOKill();
            rectTransform.DOAnchorPos(ShownPosition, TransitionDuration);
        }

        public void Hide() {
            rectTransform.DOKill();
            rectTransform.DOAnchorPos(HiddenPosition, TransitionDuration);
        }
    }
}