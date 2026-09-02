using System.Collections.Generic;
using System.Linq;
using _Rogues_Path._Game;
using _Rogues_Path.Crafting;
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

        public List<EquipmentBase> Inventory = new();
        public int InventorySpaces = 2;

        /// <summary>
        /// Rebuilds the Pawn's local inventory mirror from the
        /// authoritative Game.PlayerInventory instance data.
        /// </summary>
        public void SyncInventoryFromGameState() {
            Inventory.Clear();

            foreach (EquipmentInstanceData instanceData in Game.Instance.PlayerInventory) {
                if (instanceData == null)
                    continue;

                if (EquipmentDatabase.TryGetByID(instanceData.EquipmentID, out EquipmentBase dbEquipment)) {

                    Inventory.Add(dbEquipment);
                }
                else {
                    Debug.LogError($"Failed to restore inventory equipment ID " + $"{instanceData.EquipmentID}.");
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

            if (modifyGameState) {
                if (Game.Instance.PlayerInventory.Count >= InventorySpaces) {
                    Debug.Log("Inventory full!");

                    return false;
                }

                EquipmentInstanceData instanceData = equipment.InstanceData;

                if (instanceData == null) {
                    instanceData = new EquipmentInstanceData(equipmentID, equipment.Quality);
                }
                else if (instanceData.EquipmentID != equipmentID) {
                    Debug.LogError($"EquipmentInstanceData mismatch for {equipment.Name}. " + $"InstanceData ID={instanceData.EquipmentID}, " + $"Database ID={equipmentID}.");

                    return false;
                }

                Game.Instance.PlayerInventory.Add(instanceData);

                SyncInventoryFromGameState();

                return true;
            }

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

            if (modifyGameState) {
                EquipmentInstanceData instanceData = equipment.InstanceData;

                if (instanceData == null) {
                    instanceData = Game.Instance.PlayerInventory.FirstOrDefault(x => x != null && x.EquipmentID == equipmentID);
                }

                if (instanceData == null || !Game.Instance.PlayerInventory.Remove(instanceData)) {

                    Debug.Log($"PlayerInventory doesn't contain {equipment.Name}.");

                    return false;
                }

                SyncInventoryFromGameState();

                return true;
            }

            if (!Inventory.Remove(dbEquipment)) {
                Debug.Log($"Inventory doesn't contain {equipment.Name}.");

                return false;
            }

            return true;
        }

        public bool TryEquip(EquipmentBase equipment, bool modifyGameState = true) {

            if (equipment == null) {
                Debug.LogError("Attempted to equip null equipment.");

                return false;
            }

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
                Debug.LogError($"Equipment type mismatch for {equipment.Name}. " + $"Live={equipment.EquipType}, " + $"Database={dbEquipment.EquipType}.");

                return false;
            }

            EquipmentPart equipType = dbEquipment.EquipType;

            currentEquipment.TryGetValue(equipType, out EquipmentBase currentLiveEquipment);

            if (currentLiveEquipment == equipment)
                return true;

            EquipmentInstanceData newInstanceData = equipment.InstanceData;

            if (newInstanceData == null) {
                newInstanceData = new EquipmentInstanceData(newEquipmentID, equipment.Quality);
                equipment.InstanceData = newInstanceData;
            }
            else if (newInstanceData.EquipmentID != newEquipmentID) {
                Debug.LogError($"EquipmentInstanceData mismatch for {equipment.Name}. " + $"InstanceData ID={newInstanceData.EquipmentID}, " + $"Database ID={newEquipmentID}.");

                return false;
            }

            equipment.Quality = newInstanceData.Quality;

            EquipmentInstanceData oldInstanceData = null;
            bool gameHasOldEquipment = false;

            if (modifyGameState) {
                gameHasOldEquipment = Game.Instance.PlayerEquipment.TryGetValue(equipType, out oldInstanceData);

                bool incomingIsInInventory = Game.Instance.PlayerInventory.Contains(newInstanceData);

                int projectedInventoryCount = Game.Instance.PlayerInventory.Count;

                if (incomingIsInInventory)
                    projectedInventoryCount--;

                if (gameHasOldEquipment)
                    projectedInventoryCount++;

                if (projectedInventoryCount > InventorySpaces) {
                    Debug.Log($"Cannot equip {equipment.Name}: " + $"inventory would become " + $"{projectedInventoryCount}/{InventorySpaces}.");

                    return false;
                }

                if (incomingIsInInventory) {
                    Game.Instance.PlayerInventory.Remove(newInstanceData);
                }

                if (gameHasOldEquipment && oldInstanceData != null) {
                    Game.Instance.PlayerInventory.Add(oldInstanceData);
                }

                Game.Instance.PlayerEquipment[equipType] = newInstanceData;

                SyncInventoryFromGameState();
            }

            if (currentLiveEquipment != null) {
                if (modifyGameState
                    && gameHasOldEquipment
                    && oldInstanceData != null
                    && currentLiveEquipment.InstanceData != null
                    && !ReferenceEquals(currentLiveEquipment.InstanceData, oldInstanceData)) {

                    Debug.LogWarning(
                        $"Runtime equipment mismatch for {equipType}. "
                        + $"Game state and runtime equipment reference "
                        + $"different EquipmentInstanceData objects. "
                        + $"Game state is being treated as authoritative.");
                }

                RemoveLiveEquipment(currentLiveEquipment);
            }

            equipment.gameObject.SetActive(false);

            equipment.Owner = this;
            equipment.InstanceData = newInstanceData;

            currentEquipment.Remove(equipType);
            currentEquipment.Add(equipType, equipment);

            Character.Equip(equipment.ItemSprite, equipment.EquipType, equipment.SpriteColor);

            equipment.ApplyModifiers(equipment.Modifiers, this);

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

            if (!currentEquipment.TryGetValue(equipType, out EquipmentBase currentLiveEquipment)) {

                Debug.Log($"No live {equipType} equipment exists on " + $"{CharacterName}.");

                return false;
            }

            if (currentLiveEquipment != equipment) {
                Debug.LogWarning($"Attempted to remove stale {equipment.Name} instance. " + $"Another live object currently represents {equipType}.");

                return false;
            }

            if (modifyGameState) {
                if (!Game.Instance.PlayerEquipment.TryGetValue(equipType, out EquipmentInstanceData equippedInstanceData)) {

                    Debug.LogError(
                        $"Game.PlayerEquipment does not contain {equipType}. " + $"Runtime equipment exists, but authoritative " + $"game state says it is not equipped.");

                    return false;
                }

                if (Game.Instance.PlayerInventory.Count >= InventorySpaces) {
                    Debug.Log($"Cannot unequip {equipment.Name}: inventory full.");

                    return false;
                }

                if (equipment.InstanceData != null && !ReferenceEquals(equipment.InstanceData, equippedInstanceData)) {

                    Debug.LogWarning($"Runtime equipment mismatch for {equipment.Name}. " + $"Game state is being treated as authoritative.");
                }

                Game.Instance.PlayerEquipment.Remove(equipType);

                Game.Instance.PlayerInventory.Add(equippedInstanceData);

                SyncInventoryFromGameState();
            }

            RemoveLiveEquipment(currentLiveEquipment);
            EventBus.Raise(
                new EquipmentUnequippedEvent {
                    Owner = this,
                    EquipType = equipType
                });

            EventBus.Raise(new PawnStatChanged());

            return true;
        }

        private void RemoveLiveEquipment(EquipmentBase equipment) {
            if (equipment == null)
                return;

            if (EquipmentDatabase.IsDatabaseEntry(equipment)) {
                Debug.LogError($"DATABASE EQUIPMENT REACHED " + $"{nameof(RemoveLiveEquipment)}: {equipment.Name}");

                return;
            }

            equipment.gameObject.SetActive(false);

            equipment.RemoveModifiers(equipment.Modifiers, this);

            Character.UnEquip(equipment.EquipType);

            currentEquipment.Remove(equipment.EquipType);

            equipment.Owner = null;

            Destroy(equipment.gameObject);
        }
    }
}
