using System.Collections.Generic;
using System.Linq;
using _Rogues_Path._Game;
using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using Assets.HeroEditor4D.Common.Scripts.Enums;
using UnityEngine;

namespace _Rogues_Path.Pawns.Scripts {
    public partial class Pawn {
        public EquipmentDictionary CurrentEquipment { get => currentEquipment; set => currentEquipment = value; }

        [SerializeField]
        private EquipmentDictionary currentEquipment = new();

        #region Inventory
        public List<EquipmentBase> Inventory = new();
        public int InventorySpaces = 2;

        public bool TryAddToInventory(EquipmentBase equipment, bool modifyGameState = true) {

            if (equipment == null) {
                Debug.LogError("Equipment null!");
                return false;
            }

            // Don't add the same runtime instance twice.
            if (Inventory.Contains(equipment))
                return true;

            if (Inventory.Count >= InventorySpaces) {
                Debug.Log("Inventory full!");
                return false;
            }

            if (!TryGetDatabaseEquipment(equipment, out EquipmentBase dbEquipment)) {
                Debug.LogError($"Failed to find {equipment.Name} in {nameof(EquipmentDatabase)}. " + "Ensure it's added.");

                return false;
            }

            int id = EquipmentDatabase.Instance.Equipment.IndexOf(dbEquipment);

            Inventory.Add(equipment);

            if (modifyGameState) {
                Game.Instance.PlayerInventory.Add(id);
            }

            return true;
        }

        public bool TryRemoveFromInventory(EquipmentBase equipment, bool modifyGameState = true) {

            if (equipment == null)
                return false;

            if (!TryGetDatabaseEquipment(equipment, out EquipmentBase dbEquipment)) {
                Debug.LogError($"Failed to find {equipment.Name} in {nameof(EquipmentDatabase)}");

                return false;
            }

            /*
             * Prefer the actual runtime instance.
             *
             * As a compatibility fallback, if somebody passes a database
             * definition instead, find an item with the same name.
             */
            int index = Inventory.IndexOf(equipment);

            if (index < 0) {
                index = Inventory.FindIndex(e => e != null && e.Name == equipment.Name);
            }

            if (index < 0) {
                Debug.Log($"Inventory doesn't contain {equipment.Name}");
                return false;
            }

            int id = EquipmentDatabase.Instance.Equipment.IndexOf(dbEquipment);

            Inventory.RemoveAt(index);

            if (modifyGameState) {
                // Remove one occurrence. Multiple copies of the same item
                // are still allowed.
                Game.Instance.PlayerInventory.Remove(id);
            }

            return true;
        }
        #endregion

        public bool TryEquip(EquipmentBase equipment, bool modifyGameState = true) {

            if (equipment == null) {
                Debug.Log("Attempted to assign null equipment.");
                return false;
            }

            if (!TryGetDatabaseEquipment(equipment, out EquipmentBase dbEquipment)) {
                Debug.LogError($"Failed to find {equipment.Name} in {nameof(EquipmentDatabase)}.");

                return false;
            }

            EquipmentPart equipType = equipment.EquipType;
            int newEquipmentID = EquipmentDatabase.Instance.Equipment.IndexOf(dbEquipment);

            currentEquipment.TryGetValue(equipType, out EquipmentBase currentlyEquipped);

            /*
             * If this exact instance is already equipped, this is effectively
             * a no-op.
             */
            if (currentlyEquipped == equipment) {
                equipment.Owner = this;
                equipment.gameObject.SetActive(true);

                if (modifyGameState) {
                    Game.Instance.PlayerEquipment.Remove(equipType);
                    Game.Instance.PlayerEquipment.Add(equipType, newEquipmentID);
                }

                return true;
            }

            bool incomingIsInInventory = Inventory.Contains(equipment);

            bool currentAlreadyInInventory = currentlyEquipped != null && Inventory.Contains(currentlyEquipped);

            /*
             * Calculate inventory size AFTER the swap.
             *
             * This fixes the case:
             *
             * Inventory = FULL
             * New sword = currently in inventory
             * Old sword = currently equipped
             *
             * That swap should be legal because removing the new sword frees
             * the space needed for the old sword.
             */
            int projectedInventoryCount = Inventory.Count;

            if (incomingIsInInventory)
                projectedInventoryCount--;

            if (currentlyEquipped != null && !currentAlreadyInInventory)
                projectedInventoryCount++;

            if (projectedInventoryCount > InventorySpaces) {
                Debug.Log($"Not enough inventory space to replace " + $"{currentlyEquipped?.Name} with {equipment.Name}.");

                return false;
            }

            int oldEquipmentID = -1;

            /*
             * Resolve EVERYTHING before changing state.
             * This prevents half-completed equipment changes.
             */
            if (currentlyEquipped != null) {
                if (!TryGetDatabaseEquipment(currentlyEquipped, out EquipmentBase oldDBEquipment)) {

                    Debug.LogError($"Failed to find currently equipped item " + $"{currentlyEquipped.Name} in EquipmentDatabase.");

                    return false;
                }

                oldEquipmentID = EquipmentDatabase.Instance.Equipment.IndexOf(oldDBEquipment);
            }

            /*
             * ------------------------------------------------------------
             * COMMIT TRANSACTION
             * ------------------------------------------------------------
             */

            // Remove the incoming runtime instance from inventory first.
            if (incomingIsInInventory) {
                Inventory.Remove(equipment);

                if (modifyGameState) {
                    Game.Instance.PlayerInventory.Remove(newEquipmentID);
                }
            }

            /*
             * Remove the ACTUAL currently equipped item.
             *
             * This is the important difference from the old implementation.
             * We do NOT call TryRemoveEquipment(dbEquipment).
             */
            if (currentlyEquipped != null) {
                currentEquipment.Remove(equipType);

                if (modifyGameState) {
                    Game.Instance.PlayerEquipment.Remove(equipType);
                }

                Character.UnEquip(equipType);

                currentlyEquipped.gameObject.SetActive(false);
                currentlyEquipped.Owner = null;

                // Move the OLD equipment into inventory.
                if (!currentAlreadyInInventory) {
                    Inventory.Add(currentlyEquipped);

                    if (modifyGameState) {
                        Game.Instance.PlayerInventory.Add(oldEquipmentID);
                    }
                }
            }

            /*
             * Equip the actual runtime instance passed to us.
             */
            equipment.Owner = this;
            equipment.gameObject.SetActive(true);

            Character.Equip(equipment.ItemSprite, equipment.EquipType, equipment.SpriteColor);

            // Synchronize local equipment state.
            currentEquipment.Remove(equipType);
            currentEquipment.Add(equipType, equipment);

            // Synchronize persistent/global equipment state.
            if (modifyGameState) {
                Game.Instance.PlayerEquipment.Remove(equipType);
                Game.Instance.PlayerEquipment.Add(equipType, newEquipmentID);
            }

            EventBus.Raise(
                new EquipmentEquippedEvent {
                    Equipment = equipment,
                    Owner = this
                });

            return true;
        }

        public bool TryRemoveEquipment(EquipmentBase equipment, bool modifyGameState = true) {

            if (equipment == null) {
                Debug.Log("Attempting to remove null equipment.");
                return false;
            }

            /*
             * The dictionary is the authority for the actual equipped
             * runtime object.
             */
            if (!currentEquipment.TryGetValue(equipment.EquipType, out EquipmentBase currentlyEquipped)) {

                Debug.Log($"{equipment.EquipType} isn't currently equipped.");

                return false;
            }

            /*
             * If it's already present because of some previous state
             * inconsistency, don't add another copy.
             */
            bool alreadyInInventory = Inventory.Contains(currentlyEquipped);

            if (!alreadyInInventory && Inventory.Count >= InventorySpaces) {

                Debug.Log($"Cannot unequip {currentlyEquipped.Name}: inventory full.");

                return false;
            }

            if (!TryGetDatabaseEquipment(currentlyEquipped, out EquipmentBase dbEquipment)) {

                Debug.LogError($"Failed to find {currentlyEquipped.Name} " + $"in {nameof(EquipmentDatabase)}.");

                return false;
            }

            int id = EquipmentDatabase.Instance.Equipment.IndexOf(dbEquipment);

            /*
             * Everything has been validated.
             * Now we can safely mutate state.
             */
            currentEquipment.Remove(currentlyEquipped.EquipType);

            if (modifyGameState) {
                /*
                 * Don't use a failed Remove as a reason to abort.
                 *
                 * currentEquipment is our runtime authority.
                 * This also heals a stale PlayerEquipment dictionary.
                 */
                Game.Instance.PlayerEquipment.Remove(currentlyEquipped.EquipType);
            }

            Character.UnEquip(currentlyEquipped.EquipType);

            currentlyEquipped.gameObject.SetActive(false);
            currentlyEquipped.Owner = null;

            if (!alreadyInInventory) {
                Inventory.Add(currentlyEquipped);

                if (modifyGameState) {
                    Game.Instance.PlayerInventory.Add(id);
                }
            }

            return true;
        }

        #region Equipment Helpers
        private bool TryGetDatabaseEquipment(EquipmentBase equipment, out EquipmentBase dbEquipment) {

            dbEquipment = null;

            if (equipment == null || EquipmentDatabase.Instance == null) {

                return false;
            }

            dbEquipment = EquipmentDatabase.Instance.Equipment.FirstOrDefault(e => e != null && e.Name == equipment.Name);

            return dbEquipment != null;
        }
        #endregion
    }
}