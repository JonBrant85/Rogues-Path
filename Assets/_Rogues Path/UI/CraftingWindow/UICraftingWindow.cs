using System.Collections.Generic;
using System.Text;
using _Rogues_Path._Game;
using _Rogues_Path.Crafting;
using _Rogues_Path.Crafting.Commands;
using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.UI.InventoryWindow;
using _Rogues_Path.UI.Slots;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using DuloGames.UI;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Rogues_Path.UI.CraftingWindow {
    public class UICraftingWindow : Singleton<UICraftingWindow> {
        [FoldoutGroup("References"), SerializeField] private UIWindow Window;
        [FoldoutGroup("References"), SerializeField] private Image EquipmentIcon;
        [FoldoutGroup("References"), SerializeField] private Text EquipmentNameText;
        [FoldoutGroup("References"), SerializeField] private Text ModifiersText;
        [FoldoutGroup("References"), SerializeField] private Image OrbIcon;
        [FoldoutGroup("References"), SerializeField] private Text OrbCountText;
        [FoldoutGroup("References"), SerializeField] private Button CraftButton;
        [FoldoutGroup("References"), SerializeField] private Orb AddModifierOrb;
        [FoldoutGroup("References"), SerializeField] private EquipmentModifierDatabase ModifierDatabase;
        [FoldoutGroup("References"), SerializeField] private Transform OrbSlotsContainer;
        [FoldoutGroup("References"), SerializeField] private UIOrbSlot OrbSlotPrefab;

        private UIOrbSlot selectedOrbSlot;
        private EquipmentInstanceData selectedEquipment;
        private bool shiftRepeatActive;

        private void Awake() {
            FillOrbSlots();
            CraftButton.onClick.AddListener(CraftButtonClicked);
            Refresh();
        }

        private void Update() {
            if (selectedOrbSlot == null)
                return;

            if (shiftRepeatActive && !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift)) {

                DeactivateOrb();
                return;
            }

            if (!Input.GetMouseButtonDown(0))
                return;

            PointerEventData pointerData = new(EventSystem.current) {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new();

            EventSystem.current.RaycastAll(pointerData, results);

            foreach (RaycastResult result in results) {
                if (result.gameObject.GetComponentInParent<UIEquipmentSlot>() != null) {

                    return;
                }
            }

            DeactivateOrb();
        }

        private void OnEnable() {
            EventBus.SubscribeTo<EquipmentSlotClicked>(EquipmentSlotClickedHandler);
        }

        private void OnDisable() {
            EventBus.UnsubscribeFrom<EquipmentSlotClicked>(EquipmentSlotClickedHandler);
        }


        private void ActivateOrb(UIOrbSlot slot) {
            if (selectedOrbSlot != null)
                selectedOrbSlot.SetActivated(false);

            selectedOrbSlot = slot;
            shiftRepeatActive = false;

            selectedOrbSlot.SetActivated(true);
        }

        private void DeactivateOrb() {
            if (selectedOrbSlot != null)
                selectedOrbSlot.SetActivated(false);

            selectedOrbSlot = null;
            shiftRepeatActive = false;
        }

        private void EquipmentSlotClickedHandler(ref EquipmentSlotClicked eventData) {

            if (selectedOrbSlot == null)
                return;

            Orb orb = selectedOrbSlot.Orb;

            if (orb == null)
                return;

            if (!TryApplyOrb(orb, eventData.InstanceData)) {

                return;
            }

            EventBus.Raise(
                new EquipmentCrafted {
                    Equipment = eventData.InstanceData
                });



            if (!Game.Instance.TryConsumeOrb(orb))
                return;

            selectedOrbSlot.RefreshCount();

            bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if (shiftHeld && Game.Instance.GetOrbCount(orb) > 0) {

                shiftRepeatActive = true;
            }
            else {
                DeactivateOrb();
            }
        }

        private void RegisterOrbSlots() {
            foreach (UIOrbSlot slot in OrbSlotsContainer.GetComponentsInChildren<UIOrbSlot>()) {
                slot.OnRightClickEvent.AddListener(OrbRightClicked);
            }
        }

        private void OrbRightClicked(UIOrbSlot slot) {
            if (slot == null || slot.Orb == null)
                return;

            if (Game.Instance.GetOrbCount(slot.Orb) <= 0)
                return;

            ActivateOrb(slot);
        }

        private void FillOrbSlots() {
            for (int i = 0; i < OrbDatabase.Instance.Orbs.Count; i++) {
                UIOrbSlot slot = Instantiate(OrbSlotPrefab, OrbSlotsContainer);
                slot.Assign(OrbDatabase.Instance.Orbs[i].Orb);
                slot.OnRightClickEvent.AddListener(OrbRightClicked);
            }
        }

        private bool TryApplyOrb(Orb orb, EquipmentInstanceData equipment) {

            if (orb == null || equipment == null)
                return false;

            if (orb.Commands == null || orb.Commands.Count == 0) {

                Debug.LogError($"{orb.Name} has no crafting commands.");

                return false;
            }

            OrbCommandContext context = new(equipment, ModifierDatabase);

            foreach (OrbCommand command in orb.Commands) {
                if (command == null)
                    continue;

                if (!command.Execute(context))
                    return false;
            }

            return true;
        }

        private void EquipmentClickedHandler(EquipmentInstanceData equipment) {

            if (selectedOrbSlot == null)
                return;

            Orb orb = selectedOrbSlot.Orb;

            if (orb == null)
                return;

            if (!TryApplyOrb(orb, equipment))
                return;

            if (!Game.Instance.TryConsumeOrb(orb))
                return;

            selectedOrbSlot.RefreshCount();

            bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if (!shiftHeld || Game.Instance.GetOrbCount(orb) <= 0) {

                DeactivateOrb();
            }
        }

        private void CraftButtonClicked() {
            Debug.Log("CRAFT | Button clicked");

            if (selectedEquipment == null) {
                Debug.LogError("CRAFT | No equipment selected.");
                return;
            }

            if (AddModifierOrb == null) {
                Debug.LogError("CRAFT | AddModifierOrb is null.");
                return;
            }

            if (ModifierDatabase == null) {
                Debug.LogError("CRAFT | ModifierDatabase is null.");
                return;
            }

            int orbCount = Game.Instance.GetOrbCount(AddModifierOrb);

            Debug.Log(
                $"CRAFT | EquipmentID={selectedEquipment.EquipmentID} | "
                + $"Orb={AddModifierOrb.Name} | "
                + $"OrbCount={orbCount} | "
                + $"CurrentCraftedModifiers={selectedEquipment.CraftedModifiers.Count}");

            if (orbCount <= 0) {
                Debug.LogError("CRAFT | Player has no orbs.");
                return;
            }

            if (!CraftingSystem.TryAddRandomModifier(selectedEquipment, ModifierDatabase)) {

                Debug.LogError("CRAFT | TryAddRandomModifier returned false.");

                return;
            }

            if (!Game.Instance.TryConsumeOrb(AddModifierOrb)) {
                Debug.LogError("CRAFT | Modifier was added, but orb consumption failed.");

                return;
            }

            Debug.Log($"CRAFT SUCCESS | EquipmentID={selectedEquipment.EquipmentID} | " + $"CraftedModifiers={selectedEquipment.CraftedModifiers.Count}");

            Refresh();
        }

        private void Refresh() {
            RefreshEquipment();
        }

        private void RefreshEquipment() {
            if (selectedEquipment == null) {
                EquipmentIcon.sprite = null;
                EquipmentIcon.enabled = false;

                EquipmentNameText.text = "Select Equipment";
                ModifiersText.text = string.Empty;

                return;
            }

            if (!EquipmentDatabase.TryGetByID(selectedEquipment.EquipmentID, out EquipmentBase databaseEquipment)) {

                return;
            }

            EquipmentIcon.sprite = databaseEquipment.Icon;
            EquipmentIcon.enabled = databaseEquipment.Icon != null;

            EquipmentNameText.text = databaseEquipment.Name;

            StringBuilder builder = new StringBuilder();

            foreach (StatAndModifierPair modifier in databaseEquipment.Modifiers) {

                builder.AppendLine($"{modifier.StatID.name}: " + $"{modifier.Modifier.Value:N1}");
            }

            if (selectedEquipment.CraftedModifiers.Count > 0) {
                builder.AppendLine();
                builder.AppendLine("CRAFTED");
            }

            foreach (RolledEquipmentModifier rolledModifier in selectedEquipment.CraftedModifiers) {

                if (!ModifierDatabase.TryGetByID(rolledModifier.ModifierID, out EquipmentModifierDefinition definition)) {

                    continue;
                }

                builder.AppendLine($"{definition.Name}: " + $"{rolledModifier.Value:N1}");
            }

            ModifiersText.text = builder.ToString();
        }

        public static void Show() {
            Instance.Window.Show();
            UIInventoryWindow.Show();

            Instance.Refresh();
        }

        public static void Hide() {
            Instance.Window.Hide();
        }
    }
}