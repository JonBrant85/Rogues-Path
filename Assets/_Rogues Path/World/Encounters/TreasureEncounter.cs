using System.Collections.Generic;
using _Rogues_Path._Game;
using _Rogues_Path.Crafting;
using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace _Rogues_Path.World.Encounters {
    [CreateAssetMenu(fileName = "New Treasure Encounter", menuName = Game.Name + "/Data/TreasureEncounterData")]
    public class TreasureEncounter : EncounterData {
        [Min(1)] public int EquipmentChoiceCount = 3;
        public string ButtonText = "Select";

        public override async UniTask HandleEncounter(Transform windowContent, Transform bottomBar, Button buttonPrefab) {
            List<EquipmentBase> equipmentChoices = GetRandomUniqueEquipment(EquipmentChoiceCount);

            if (equipmentChoices.Count == 0) {
                Debug.LogError("Treasure encounter could not find any equipment choices.");
                return;
            }

            EquipmentInstanceData selectedEquipment = await UIEncounterWindow.Instance.WaitForEquipmentSelection(
                equipmentChoices,
                ButtonText);

            if (selectedEquipment == null)
                return;

            Game.Instance.PlayerInventory.Add(selectedEquipment);
            EventBus.Raise(new InventoryChanged());

            if (EquipmentDatabase.TryGetByID(selectedEquipment.EquipmentID, out EquipmentBase equipment))
                Debug.Log($"Treasure encounter granted {equipment.Name}.");
        }

        private static List<EquipmentBase> GetRandomUniqueEquipment(int count) {
            List<EquipmentBase> availableEquipment = EquipmentDatabase.Instance != null
                ? EquipmentDatabase.Instance.Equipment
                : new List<EquipmentBase>();

            availableEquipment.RemoveAll(equipment => equipment == null);

            int choiceCount = Mathf.Min(count, availableEquipment.Count);

            for (int i = 0; i < choiceCount; i++) {
                int randomIndex = Random.Range(i, availableEquipment.Count);
                (availableEquipment[i], availableEquipment[randomIndex]) =
                    (availableEquipment[randomIndex], availableEquipment[i]);
            }

            if (choiceCount < availableEquipment.Count)
                availableEquipment.RemoveRange(choiceCount, availableEquipment.Count - choiceCount);

            return availableEquipment;
        }
    }
}
