using System;
using _Rogues_Path._Game;
using _Rogues_Path.Crafting;
using DuloGames.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Rogues_Path.UI.CraftingWindow {
    [Serializable]
    public class OnOrbSlotClickEvent : UnityEvent<UIOrbSlot> {}

    public class UIOrbSlot : UISlotBase {
        public Orb Orb;
        public OnOrbSlotClickEvent OnRightClickEvent = new();

        [SerializeField]
        private Text CountText;
        [SerializeField]
        private GameObject ActivatedHighlight;

        public bool IsActivated { get; private set; }

        
        public bool Assign(Orb orb) {
            if (orb == null) {
                Debug.LogError($"Attempted to assign a null Orb to {name}.");
                return false;
            }

            Orb = orb;
            base.Assign(Orb.Icon);
            RefreshCount();
            return true;
        }

        public override bool Assign(UnityEngine.Object source) {
            if (source is not Orb orb)
                return false;

            return Assign(orb);
        }

        public override void Unassign() {
            base.Unassign();
            Orb = null;
            if (CountText != null)
                CountText.text = string.Empty;
        }

        public override bool IsAssigned() {
            return Orb != null;
        }
        
        public void SetCount(int count) {
            if (CountText == null)
                return;

            CountText.text = count.ToString();
        }

        public void RefreshCount() {
            if (CountText == null)
                return;

            if (Orb == null) {
                CountText.text = string.Empty;
                return;
            }

            CountText.text = Game.Instance.GetOrbCount(Orb).ToString();
        }

        public override void OnPointerDown(PointerEventData eventData) {
            base.OnPointerDown(eventData);

            if (eventData.button != PointerEventData.InputButton.Right) {

                return;
            }

            if (Orb == null)
                return;

            if (Game.Instance.GetOrbCount(Orb) <= 0)
                return;

            OnRightClickEvent.Invoke(this);
        }

        public override void OnTooltip(bool show) {
            UITooltip.InstantiateIfNecessary(gameObject);

            if (!IsAssigned())
                return;

            if (show) {
                PrepareTooltip();

                UITooltip.AnchorToRect(transform as RectTransform);

                UITooltip.Show();
            }
            else {
                UITooltip.Hide();
            }
        }

        private void PrepareTooltip() {
            UITooltip.AddTitle(Orb.Name);

            UITooltip.AddSpacer();

            UITooltip.AddLine($"Owned: {Game.Instance.GetOrbCount(Orb)}", "ItemAttribute");

            if (!string.IsNullOrEmpty(Orb.Description)) {
                UITooltip.AddSpacer();

                UITooltip.AddLine(Orb.Description, "ItemDescription");
            }

            UITooltip.AddSpacer();

            UITooltip.AddLine("Right-click to use", "ItemAttribute");
        }
        
        public void SetActivated(bool activated) {
            IsActivated = activated;

            if (ActivatedHighlight != null)
                ActivatedHighlight.SetActive(activated);
        }
    }
}