using System.Linq;
using _Rogues_Path.Pawns.Scripts;
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

        private void Awake() {
            rectTransform = GetComponent<RectTransform>();
        }

        private void OnEnable() {
            EventBus.SubscribeTo<CombatEncounterStarted>(CombatEncounterStartedEventHandler);
            EventBus.SubscribeTo<CombatEncounterEnded>(CombatEncounterEndedEventHandler);
        }

        private void OnDisable() {
            EventBus.UnsubscribeFrom<CombatEncounterStarted>(CombatEncounterStartedEventHandler);
            EventBus.UnsubscribeFrom<CombatEncounterEnded>(CombatEncounterEndedEventHandler);
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