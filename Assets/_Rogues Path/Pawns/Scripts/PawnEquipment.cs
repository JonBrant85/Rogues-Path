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


        /// <summary>
        /// Rebuilds the Pawn's local inventory mirror from the
        /// authoritative Game.PlayerInventory IDs.
        /// </summary>
        public void SyncInventoryFromGameState() {
            Inventory.Clear();

            foreach (int ID in Game.Instance.PlayerInventory) {
                if (EquipmentDatabase.TryGetByID(ID, out EquipmentBase dbEquipment)) {

                    Inventory.Add(dbEquipment);
                }
                else {
                    Debug.LogError($"Failed to restore inventory equipment ID {ID}.");
                }
            }
        }


        public bool TryAddToInventory(EquipmentBase equipment, bool modifyGameState = true) {

            if (equipment == null) {
                Debug.LogError("Equipment null!");
                return false;
            }

            if (!EquipmentDatabase.TryFind(equipment, out EquipmentBase dbEquipment)) {

                Debug.LogError($"Failed to find {equipment.Name} in " + $"{nameof(EquipmentDatabase)}.");

                return false;
            }

            if (!EquipmentDatabase.TryGetID(dbEquipment, out int equipmentID)) {

                return false;
            }

            /*
             * Game.PlayerInventory is authoritative.
             */
            if (modifyGameState) {
                if (Game.Instance.PlayerInventory.Count >= InventorySpaces) {

                    Debug.Log("Inventory full!");
                    return false;
                }

                Game.Instance.PlayerInventory.Add(equipmentID);

                SyncInventoryFromGameState();

                return true;
            }

            /*
             * Local-only operation.
             *
             * Useful while constructing a Pawn from already-existing
             * Game state.
             */
            if (Inventory.Count >= InventorySpaces) {
                Debug.Log("Inventory full!");
                return false;
            }

            Inventory.Add(dbEquipment);

            return true;
        }


        public bool TryRemoveFromInventory(EquipmentBase equipment, bool modifyGameState = true) {

            if (equipment == null)
                return false;

            if (!EquipmentDatabase.TryFind(equipment, out EquipmentBase dbEquipment)) {

                Debug.LogError($"Failed to find {equipment.Name} in " + $"{nameof(EquipmentDatabase)}.");

                return false;
            }

            if (!EquipmentDatabase.TryGetID(dbEquipment, out int equipmentID)) {

                return false;
            }

            /*
             * Game state is authoritative.
             */
            if (modifyGameState) {
                if (!Game.Instance.PlayerInventory.Remove(equipmentID)) {

                    Debug.Log($"PlayerInventory doesn't contain {equipment.Name}.");

                    return false;
                }

                SyncInventoryFromGameState();

                return true;
            }

            /*
             * Local inventory contains DB references,
             * so reference-based removal is valid here.
             */
            if (!Inventory.Remove(dbEquipment)) {
                Debug.Log($"Inventory doesn't contain {equipment.Name}.");

                return false;
            }

            return true;
        }
        #endregion


        #region Equipment
        public bool TryEquip(EquipmentBase equipment, bool modifyGameState = true) {

            if (equipment == null) {
                Debug.LogError("Attempted to equip null equipment.");

                return false;
            }

            /*
             * Database templates must NEVER become live equipment.
             */
            if (EquipmentDatabase.IsDatabaseEntry(equipment)) {
                Debug.LogError($"Attempted to equip database template " + $"{equipment.Name} directly. " + $"Create a live instance with EquipmentDatabase first.");

                return false;
            }

            if (!EquipmentDatabase.TryFind(equipment, out EquipmentBase dbEquipment)) {

                Debug.LogError($"Failed to find {equipment.Name} in EquipmentDatabase.");

                return false;
            }

            if (!EquipmentDatabase.TryGetID(dbEquipment, out int newEquipmentID)) {

                return false;
            }

            if (equipment.EquipType != dbEquipment.EquipType) {
                Debug.LogError($"Equipment type mismatch for {equipment.Name}. " + $"Live = {equipment.EquipType}, " + $"Database = {dbEquipment.EquipType}.");

                return false;
            }

            EquipmentPart equipType = dbEquipment.EquipType;

            currentEquipment.TryGetValue(equipType, out EquipmentBase currentLiveEquipment);

            if (currentLiveEquipment == equipment) {
                return true;
            }


            int oldEquipmentID = -1;
            bool gameHasOldEquipment = false;

            /*
             * =========================================================
             * AUTHORITATIVE GAME-STATE TRANSACTION
             * =========================================================
             */

            if (modifyGameState) {
                gameHasOldEquipment = Game.Instance.PlayerEquipment.TryGetValue(equipType, out oldEquipmentID);

                bool incomingIsInInventory = Game.Instance.PlayerInventory.Contains(newEquipmentID);

                /*
                 * Calculate inventory count AFTER the transaction.
                 *
                 * If the incoming item came from inventory:
                 *     -1
                 *
                 * If we're replacing equipped equipment:
                 *     +1
                 */
                int projectedInventoryCount = Game.Instance.PlayerInventory.Count;

                if (incomingIsInInventory) {
                    projectedInventoryCount--;
                }

                if (gameHasOldEquipment) {
                    projectedInventoryCount++;
                }

                if (projectedInventoryCount > InventorySpaces) {

                    Debug.Log($"Cannot equip {equipment.Name}: " + $"inventory would become " + $"{projectedInventoryCount}/{InventorySpaces}.");

                    return false;
                }

                /*
                 * Commit authoritative inventory state.
                 */
                if (incomingIsInInventory) {
                    Game.Instance.PlayerInventory.Remove(newEquipmentID);
                }

                if (gameHasOldEquipment) {
                    Game.Instance.PlayerInventory.Add(oldEquipmentID);
                }

                /*
                 * Commit authoritative equipment state.
                 */
                Game.Instance.PlayerEquipment.Remove(equipType);

                Game.Instance.PlayerEquipment.Add(equipType, newEquipmentID);

                /*
                 * Rebuild the local/template mirror from the now-correct
                 * authoritative inventory state.
                 */
                SyncInventoryFromGameState();
            }


            /*
             * =========================================================
             * REMOVE OLD LIVE REPRESENTATION
             * =========================================================
             */

            if (currentLiveEquipment != null) {
                if (EquipmentDatabase.TryGetID(currentLiveEquipment, out int currentLiveID) && modifyGameState && gameHasOldEquipment && currentLiveID != oldEquipmentID) {

                    Debug.LogWarning(
                        $"Runtime equipment mismatch for {equipType}. "
                        + $"Game state says ID {oldEquipmentID}, "
                        + $"runtime object is ID {currentLiveID}. "
                        + $"Game state is being treated as authoritative.");
                }

                RemoveLiveEquipment(currentLiveEquipment);
            }


            /*
             * =========================================================
             * MATERIALIZE NEW RUNTIME REPRESENTATION
             * =========================================================
             */

            /*
             * Ensure it cannot receive events until its runtime state
             * is fully configured.
             */
            equipment.gameObject.SetActive(false);

            equipment.Owner = this;

            currentEquipment.Remove(equipType);

            currentEquipment.Add(equipType, equipment);

            Character.Equip(equipment.ItemSprite, equipment.EquipType, equipment.SpriteColor);

            equipment.ApplyModifiers(equipment.Modifiers, this);

            /*
             * Last step:
             * enable event participation.
             */
            equipment.gameObject.SetActive(true);

            EventBus.Raise(
                new EquipmentEquippedEvent {
                    Equipment = equipment,
                    Owner = this
                });

            EventBus.Raise(new PawnStatChanged());

            return true;
        }


        public bool TryRemoveEquipment(EquipmentBase equipment, bool modifyGameState = true) {

            if (equipment == null) {
                Debug.LogError("Attempted to remove null equipment.");

                return false;
            }

            if (EquipmentDatabase.IsDatabaseEntry(equipment)) {
                Debug.LogError($"Attempted to unequip database template " + $"{equipment.Name}.");

                return false;
            }

            EquipmentPart equipType = equipment.EquipType;

            /*
             * The game state tells us WHAT should be equipped.
             *
             * currentEquipment tells us WHICH live object currently
             * represents it in this scene.
             */
            if (!currentEquipment.TryGetValue(equipType, out EquipmentBase currentLiveEquipment)) {

                Debug.Log($"No live {equipType} equipment exists on " + $"{CharacterName}.");

                return false;
            }

            if (currentLiveEquipment != equipment) {
                Debug.LogWarning($"Attempted to remove stale {equipment.Name} instance. " + $"Another live object currently represents {equipType}.");

                return false;
            }


            /*
             * =========================================================
             * AUTHORITATIVE GAME-STATE TRANSACTION
             * =========================================================
             */

            if (modifyGameState) {
                if (!Game.Instance.PlayerEquipment.TryGetValue(equipType, out int equippedID)) {

                    Debug.LogError(
                        $"Game.PlayerEquipment does not contain {equipType}. " + $"Runtime equipment exists, but authoritative " + $"game state says it is not equipped.");

                    return false;
                }

                if (Game.Instance.PlayerInventory.Count >= InventorySpaces) {

                    Debug.Log($"Cannot unequip {equipment.Name}: inventory full.");

                    return false;
                }

                Game.Instance.PlayerEquipment.Remove(equipType);

                Game.Instance.PlayerInventory.Add(equippedID);

                SyncInventoryFromGameState();
            }


            /*
             * Remove runtime representation.
             */
            RemoveLiveEquipment(currentLiveEquipment);

            EventBus.Raise(new PawnStatChanged());

            return true;
        }


        private void RemoveLiveEquipment(EquipmentBase equipment) {
            if (equipment == null)
                return;

            bool isDatabaseEntry = EquipmentDatabase.IsDatabaseEntry(equipment);

            Debug.Log(
                $"REMOVE LIVE EQUIPMENT\n"
                + $"Name: {equipment.Name}\n"
                + $"Type: {equipment.EquipType}\n"
                + $"Instance ID: {equipment.GetInstanceID()}\n"
                + $"Is Database Entry: {isDatabaseEntry}\n"
                + $"Scene Valid: {equipment.gameObject.scene.IsValid()}\n"
                + $"Scene: {equipment.gameObject.scene.name}\n"
                + $"Stack:\n{System.Environment.StackTrace}");

            if (isDatabaseEntry) {
                Debug.LogError($"DATABASE EQUIPMENT REACHED RemoveLiveEquipment: " + $"{equipment.Name}");

                return;
            }

            equipment.gameObject.SetActive(false);

            equipment.RemoveModifiers(equipment.Modifiers, this);

            Character.UnEquip(equipment.EquipType);

            currentEquipment.Remove(equipment.EquipType);

            equipment.Owner = null;

            Destroy(equipment.gameObject);
        }
        #endregion
    }
}