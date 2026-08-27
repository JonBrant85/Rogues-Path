using System.Text;
using _Rogues_Path._Game;
using _Rogues_Path.Crafting;
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

        private EquipmentInstanceData selectedEquipment;

        private void Awake() {
            CraftButton.onClick.AddListener(CraftButtonClicked);

            ClearSelection();
            RefreshOrb();
        }

        private void OnEnable() {
            UIInventoryWindow.EquipmentClicked += EquipmentClickedHandler;
        }

        private void OnDisable() {
            UIInventoryWindow.EquipmentClicked -= EquipmentClickedHandler;
        }

        private void EquipmentClickedHandler(EquipmentInstanceData instanceData) {

            selectedEquipment = instanceData;

            RefreshEquipment();
            RefreshOrb();
        }

        private void CraftButtonClicked() {
            if (selectedEquipment == null)
                return;

            if (AddModifierOrb == null)
                return;

            if (ModifierDatabase == null)
                return;

            if (!Game.Instance.TryConsumeOrb(AddModifierOrb))
                return;

            if (!CraftingSystem.TryAddRandomModifier(selectedEquipment, ModifierDatabase)) {

                Game.Instance.AddOrb(AddModifierOrb);

                return;
            }

            RefreshEquipment();
            RefreshOrb();

            EventBus.Raise(new InventoryChanged());
        }

        private void RefreshEquipment() {
            if (selectedEquipment == null) {
                ClearSelection();
                return;
            }

            if (!EquipmentDatabase.TryGetByID(selectedEquipment.EquipmentID, out EquipmentBase databaseEquipment)) {

                ClearSelection();
                return;
            }

            EquipmentIcon.sprite = databaseEquipment.Icon;
            EquipmentIcon.enabled = databaseEquipment.Icon != null;

            EquipmentNameText.text = databaseEquipment.Name;

            StringBuilder builder = new StringBuilder();

            foreach (StatAndModifierPair modifier in databaseEquipment.Modifiers) {

                builder.AppendLine($"{modifier.StatID.name}: " + $"{modifier.Modifier.Value:N1}");
            }

            foreach (RolledEquipmentModifier rolledModifier in selectedEquipment.CraftedModifiers) {

                if (!ModifierDatabase.TryGetByID(rolledModifier.ModifierID, out EquipmentModifierDefinition definition)) {

                    continue;
                }

                string modifierName = string.IsNullOrEmpty(definition.Name) ? definition.StatID.name : definition.Name;

                builder.AppendLine($"{modifierName}: " + $"{rolledModifier.Value:N1}");
            }

            ModifiersText.text = builder.ToString();
        }

        private void RefreshOrb() {
            if (AddModifierOrb == null) {
                OrbIcon.sprite = null;
                OrbIcon.enabled = false;

                OrbCountText.text = "0";
                CraftButton.interactable = false;

                return;
            }

            OrbIcon.sprite = AddModifierOrb.Icon;
            OrbIcon.enabled = AddModifierOrb.Icon != null;

            int count = Game.Instance.GetOrbCount(AddModifierOrb);

            OrbCountText.text = count.ToString();

            CraftButton.interactable = selectedEquipment != null && count > 0;
        }

        private void ClearSelection() {
            selectedEquipment = null;

            EquipmentIcon.sprite = null;
            EquipmentIcon.enabled = false;

            EquipmentNameText.text = "Select Equipment";
            ModifiersText.text = string.Empty;

            CraftButton.interactable = false;
        }

        public static void Show() {
            Instance.Window.Show();

            UIInventoryWindow.Show();

            Instance.RefreshOrb();
        }

        public static void Hide() {
            Instance.Window.Hide();
        }
    }
}