using System;
using _Rogues_Path._Game;
using _Rogues_Path.Crafting;
using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.Pawns;
using _Rogues_Path.Pawns.Scripts;
using _Rogues_Path.UI.Slots;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using DuloGames.UI;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Rogues_Path.UI.InventoryWindow {
    public class UIInventoryWindow : Singleton<UIInventoryWindow> {
        [SerializeField] private UIWindow Window;
        [SerializeField] private Transform SlotsContainer;
        [SerializeField] private EquipmentModifierDatabase ModifierDatabase;

        private void Awake() {
            FillSlotsWithInventory();
        }

        private void OnEnable() {
            EventBus.SubscribeTo<InventoryChanged>(InventoryChangedHandler);
        }

        private void OnDisable() {
            EventBus.UnsubscribeFrom<InventoryChanged>(InventoryChangedHandler);
        }

        [Button]
        private void InventoryChangedHandler(ref InventoryChanged eventData) {
            ClearAllSlots();
            FillSlotsWithInventory();
        }

        private void FillSlotsWithInventory() {
            for (int i = 0; i < SlotsContainer.childCount && i < Game.Instance.PlayerInventory.Count; i++) {

                EquipmentInstanceData instanceData = Game.Instance.PlayerInventory[i];

                if (instanceData == null)
                    continue;

                Transform child = SlotsContainer.GetChild(i);

                if (!child.TryGetComponent(out UIEquipmentSlot slot))
                    continue;

                if (!EquipmentDatabase.TryGetByID(instanceData.EquipmentID, out EquipmentBase equipment)) {

                    continue;
                }

                slot.Assign(equipment);

                slot.BindInstanceData(instanceData, ModifierDatabase);
            }
        }

        private void ClearAllSlots() {
            for (int i = 0; i < SlotsContainer.childCount; i++) {
                var child = SlotsContainer.GetChild(i);

                if (child.TryGetComponent(out UIEquipmentSlot slot)) {
                    slot.ClearUIReference();
                }
            }
        }

        public static void Show() {
            Instance.Window.Show();
        }

        public static void Hide() {
            Instance.Window.Hide();
        }
    }
}