using System.Text;
using _Rogues_Path._Game;
using _Rogues_Path.Crafting;
using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.UI.CharacterScreen;
using _Rogues_Path.UI.InventoryWindow;
using _Rogues_Path.Utilities;
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
            Game.Instance.AddOrb(AddModifierOrb, 10);
            Refresh();
        }

        private void OnEnable() {
            UIInventoryWindow.EquipmentClicked += EquipmentClickedHandler;
        }

        private void OnDisable() {
            UIInventoryWindow.EquipmentClicked -= EquipmentClickedHandler;
        }

        private void EquipmentClickedHandler(EquipmentInstanceData instanceData) {

            selectedEquipment = instanceData;

            Refresh();
        }

        private void CraftButtonClicked() {
            if (selectedEquipment == null)
                return;

            if (AddModifierOrb == null)
                return;

            if (ModifierDatabase == null)
                return;

            if (Game.Instance.GetOrbCount(AddModifierOrb) <= 0)
                return;

            if (!CraftingSystem.TryAddRandomModifier(selectedEquipment, ModifierDatabase)) {

                return;
            }

            if (!Game.Instance.TryConsumeOrb(AddModifierOrb)) {
                Debug.LogError("Crafting succeeded but failed to consume orb.");

                return;
            }

            Refresh();
        }

        private void Refresh() {
            RefreshEquipment();
            RefreshOrb();
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