using System.Collections.Generic;
using System.Linq;
using _Rogues_Path._Game;
using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using UnityEngine;

namespace _Rogues_Path.Pawns {
    public partial class Pawn {
        public EquipmentDictionary CurrentEquipment { get => currentEquipment; }
        [SerializeField] private EquipmentDictionary currentEquipment = new();

        #region Inventory. Move to new file
        public List<EquipmentBase> Inventory = new();
        public int InventorySpaces = 2;

        public bool TryAddToInventory(EquipmentBase equipment, bool modifyGameState = true) {
            if (Inventory.Count >= InventorySpaces) {
                Debug.Log($"Inventory full!");
                return false;
            }

            if (equipment == null) {
                Debug.LogError("Equipment null!");
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
            var dbEntry = EquipmentDatabase.Instance.Equipment.FirstOrDefault(e => e.Name == equipment.Name);
            Debug.Assert(dbEntry != null);

            if (currentEquipment.ContainsKey(equipment.EquipType)) {
                // Move existing item to inventory if possible
                if (Inventory.Count + 1 > InventorySpaces) {
                    Debug.Log($"Not enough inventory spaces to move {dbEntry.Name} to inventory!");
                    return false;
                }
                else {
                    return TryRemoveEquipment(dbEntry) && TryAddToInventory(dbEntry);
                }
            }
            else {
                // Set owner and update sprite
                equipment.Owner = this;
                equipment.gameObject.SetActive(true);
                Debug.Log($"Equipping to character");
                character.Equip(equipment.Sprite, equipment.EquipType, equipment.SpriteColor);

                // Raise a new EquipmentEquipped event
                EventBus.Raise(
                    new EquipmentEquippedEvent {
                        Equipment = dbEntry,
                        Owner = this
                    });

                // Add to dictionary
                currentEquipment.Add(dbEntry.EquipType, dbEntry);

                // Update game state if necessary
                
                /*
                if (modifyGameState) {
                    Game.PlayerEquipment.Add(dbEntry.EquipType, dbEntry);
                }*/

                return true;
            }
        }

        public bool TryRemoveEquipment(EquipmentBase equipment, bool modifyGameState = true) {
            if (equipment == null) return false;

            character.UnEquip(equipment.EquipType);
            currentEquipment.Remove(equipment.EquipType);
            /*
            if (modifyGameState) {
                Game.PlayerEquipment.Remove(equipment.EquipType);
            }*/

            equipment.gameObject.SetActive(false);
            return true;
        }
    }
}