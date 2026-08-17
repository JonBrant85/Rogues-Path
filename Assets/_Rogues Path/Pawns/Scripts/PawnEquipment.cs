using System.Collections.Generic;
using System.Linq;
using _Rogues_Path._Game;
using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using Bewildered.SmartLibrary;
using UnityEngine;

namespace _Rogues_Path.Pawns {
    public partial class Pawn {
        public EquipmentDictionary CurrentEquipment { get => currentEquipment; }
        [SerializeField] private EquipmentDictionary currentEquipment = new();

        #region Inventory. Move to new file
        public List<EquipmentBase> Inventory = new();
        public int InventorySpaces = 2;

        public bool TryAddToInventory(EquipmentBase equipment, bool modifyGameState = true) {
            if (equipment == null) {
                Debug.LogError("Equipment null!");
                return false;
            }

            if (Inventory.Count >= InventorySpaces) {
                Debug.Log($"Inventory full!");
                return false;
            }

            var equipmentDBEntry = EquipmentDatabase.Instance.Equipment.FirstOrDefault(entry => entry.Name == equipment.Name);

            if (equipmentDBEntry == null) {
                Debug.LogError($"Failed to find {equipment.Name} in {nameof(EquipmentDatabase)}. Ensure it's added");
                return false;
            }

            Inventory.Add(equipment);
            /*
            if (modifyGameState) {
                Game.PlayerInventory.Add(equipment);
            }
            */

            return true;
        }

        public bool TryRemoveFromInventory(EquipmentBase equipment, bool modifyGameState = true) {
            if (equipment == null) return false;
            var dbEntry = EquipmentDatabase.Instance.Equipment.FirstOrDefault(e => e.Name == equipment.Name);

            if (dbEntry == null) {
                Debug.LogError($"Failed to find {equipment.Name} in {nameof(EquipmentDatabase)}");
            }

            if (!Inventory.Contains(dbEntry)) {
                Debug.Log($"Inventory doesn't contain {equipment.Name}");
                return false;
            }

            Inventory.Remove(dbEntry);

            /*
             if (modifyGameState) {
                Game.PlayerInventory.Remove(dbEntry);
            }
            */

            return true;
        }
        #endregion

        public bool TryEquip(EquipmentBase equipment, bool modifyGameState = true) {
            if (equipment == null) {
                Debug.Log($"Attempted to assign a null equipment");
                return false;
            }


            // Find Database entry and use that instead of live equipment
            var dbEquipment = EquipmentDatabase.Instance.Equipment.FirstOrDefault(e => e.Name == equipment.Name);
            Debug.Assert(dbEquipment != null);

            // If something's already in the equipment slot, try to move it to inventory
            if (Game.Instance.PlayerEquipment.ContainsKey(dbEquipment.EquipType)) {
                // Move existing item to inventory if possible
                if (Inventory.Count + 1 > InventorySpaces) {
                    Debug.Log($"Not enough inventory spaces to move {dbEquipment.Name} to inventory!");
                    return false;
                }
                else {
                    // If successfully removed and added to inventory, equip it
                    if (TryRemoveEquipment(dbEquipment, modifyGameState) && TryAddToInventory(dbEquipment, modifyGameState)) {
                        EquipEquipment();
                        return true;
                    }
                    else {
                        return false;
                    }
                }
            }
            // If the slot isn't occupied, take it
            else {
                EquipEquipment();
                return true;
            }

            void EquipEquipment() {
                // Set owner and update sprite
                dbEquipment.Owner = this;
                dbEquipment.gameObject.SetActive(true);
                Character.Equip(dbEquipment.Sprite, dbEquipment.EquipType, dbEquipment.SpriteColor);

                // Add to dictionary
                currentEquipment.Add(dbEquipment.EquipType, dbEquipment);

                // Update game state if necessary
                if (modifyGameState) {
                    int ID = EquipmentDatabase.Instance.Equipment.IndexOf(dbEquipment);
                    Game.Instance.PlayerEquipment.Add(dbEquipment.EquipType, ID);
                }

                // Raise a new EquipmentEquipped event
                EventBus.Raise(
                    new EquipmentEquippedEvent {
                        Equipment = dbEquipment,
                        Owner = this
                    });
            }
        }

        public bool TryRemoveEquipment(EquipmentBase equipment, bool modifyGameState = true) {
            // If the equipment is null, or can't be removed from local/global inventory, return false and do nothing
            if (equipment == null) return false;
            if (!currentEquipment.Remove(equipment.EquipType)) return false;
            if (modifyGameState && !Game.Instance.PlayerEquipment.Remove(equipment.EquipType)) return false;

            Character.UnEquip(equipment.EquipType);
            equipment.gameObject.SetActive(false);
            return true;
        }
    }
}