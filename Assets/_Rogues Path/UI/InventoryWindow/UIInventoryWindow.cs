using System;
using _Rogues_Path._Game;
using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.Pawns;
using _Rogues_Path.UI.Slots;
using _Rogues_Path.Utilities;
using UnityEngine;

namespace _Rogues_Path.UI.InventoryWindow {
    public class UIInventoryWindow : Singleton<UIInventoryWindow> {
        [SerializeField] private Transform SlotsContainer;


        private void OnEnable() {
            for (int i = 0; i < SlotsContainer.childCount; i++) {
                var child = SlotsContainer.GetChild(i);

                if (child.TryGetComponent(out UIEquipmentSlot equipSlot)) {
                    equipSlot.OnAssignEvent.AddListener(OnAssignEvent);
                    equipSlot.OnUnassignEvent.AddListener(OnUnassignEvent);
                }
                else {
                    Debug.Log($"Failed to find component!");
                }
            }
            
            void OnAssignEvent(Pawn owner, EquipmentBase equipment) {
                if (EquipmentDatabase.GetIDByName(equipment.Name, out int ID)) {
                    Game.Instance.PlayerInventory.Add(ID);
                }
                else {
                    Debug.Log($"Failed to get Equipment ['{equipment.Name}'] from DB. Ensure it's added");
                }
            }

            void OnUnassignEvent(Pawn owner, EquipmentBase equipment) {
                if (EquipmentDatabase.GetIDByName(equipment.Name, out int ID)) {
                    Game.Instance.PlayerInventory.Remove(ID);
                }
                else {
                    Debug.Log($"Failed to get Equipment ['{equipment.Name}'] from DB. Ensure it's added");
                }
            }
        }

        
    }
}