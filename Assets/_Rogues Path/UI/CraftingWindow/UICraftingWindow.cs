using System.Text;
using _Rogues_Path._Game;
using _Rogues_Path.Crafting;
using _Rogues_Path.Crafting.Commands;
using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.UI.InventoryWindow;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using DuloGames.UI;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace _Rogues_Path.UI.CraftingWindow {
    public class UICraftingWindow : Singleton<UICraftingWindow> {
        [FoldoutGroup("References"), SerializeField]
        private UIWindow Window;

        [FoldoutGroup("References"), SerializeField]
        private Image EquipmentIcon;

        [FoldoutGroup("References"), SerializeField]
        private Text EquipmentNameText;

        [FoldoutGroup("References"), SerializeField]
        private Text ModifiersText;

        [FoldoutGroup("References"), SerializeField]
        private Image OrbIcon;

        [FoldoutGroup("References"), SerializeField]
        private Text OrbCountText;

        [FoldoutGroup("References"), SerializeField]
        private Button CraftButton;

        [FoldoutGroup("References"), SerializeField]
        private Orb AddModifierOrb;

        [FoldoutGroup("References"), SerializeField]
        private EquipmentModifierDatabase ModifierDatabase;

        [FoldoutGroup("References"), SerializeField]
        private Transform OrbSlotsContainer;

        private UIOrbSlot selectedOrbSlot;
        private EquipmentInstanceData selectedEquipment;

        private void Awake() {
            RegisterOrbSlots();
            FillOrbSlots();
            CraftButton.onClick.AddListener(CraftButtonClicked);
            Game.Instance.AddOrb(AddModifierOrb, 10);
            Refresh();
        }

        private void OnEnable() {
            UIInventoryWindow.EquipmentClicked += EquipmentClickedHandler;
        }

        private void OnDisable() {
            UIInventoryWindow.EquipmentClicked -= EquipmentClickedHandler;
        }

        private void RegisterOrbSlots() {
            foreach (UIOrbSlot slot in OrbSlotsContainer.GetComponentsInChildren<UIOrbSlot>()) {
                slot.OnRightClickEvent.AddListener(OrbRightClicked);
            }

            void OrbRightClicked(UIOrbSlot slot) {
                if (slot == null || slot.Orb == null)
                    return;

                if (Game.Instance.GetOrbCount(slot.Orb) <= 0)
                    return;

                selectedOrbSlot = slot;
            }
        }

        private void FillOrbSlots() {
            var orbs = OrbDatabase.Instance.Orbs;

            for (int i = 0; i < OrbSlotsContainer.childCount && i < orbs.Count; i++) {

                UIOrbSlot slot = OrbSlotsContainer.GetChild(i).GetComponent<UIOrbSlot>();

                if (slot == null)
                    continue;

                slot.Assign(orbs[i]);
            }
        }

        private bool TryApplyOrb(Orb orb, EquipmentInstanceData equipment) {

            if (orb == null || equipment == null)
                return false;

            if (orb.Command == null) {
                Debug.LogError($"{orb.Name} has no crafting command.");

                return false;
            }

            OrbCommandContext context = new(equipment, ModifierDatabase);

            return orb.Command.Execute(context);
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

                selectedOrbSlot = null;
            }
        }

        [Button]
        private void GiveTestOrbs() {
            if (AddModifierOrb == null) {
                Debug.LogError("AddModifierOrb is null.");
                return;
            }

            if (!Game.Instance.AddOrb(AddModifierOrb, 10)) {
                Debug.LogError($"Failed to add orb: {AddModifierOrb.Name}");

                return;
            }

            Debug.Log($"Added 10 {AddModifierOrb.Name}. " + $"Count={Game.Instance.GetOrbCount(AddModifierOrb)}");

            CraftButton.interactable = true;

            Refresh();
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