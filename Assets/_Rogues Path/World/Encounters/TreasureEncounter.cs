using System.Collections.Generic;
using _Rogues_Path._Game;
using _Rogues_Path.Crafting;
using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using Cysharp.Threading.Tasks;
using DuloGames.UI;
using UnityEngine;
using UnityEngine.UI;

namespace _Rogues_Path.World.Encounters {
    [CreateAssetMenu(fileName = "New Treasure Encounter", menuName = Game.Name + "/Data/TreasureEncounterData")]
    public class TreasureEncounter : EncounterData {
        [Min(1)] public int EquipmentChoiceCount = 3;
        public string ButtonText = "Select";
        [Min(0)] public int PoorWeight = 10;
        [Min(0)] public int CommonWeight = 40;
        [Min(0)] public int UncommonWeight = 25;
        [Min(0)] public int RareWeight = 15;
        [Min(0)] public int EpicWeight = 7;
        [Min(0)] public int LegendaryWeight = 3;

        public override async UniTask HandleEncounter(Transform windowContent, Transform bottomBar, Button buttonPrefab) {
            List<EquipmentInstanceData> equipmentChoices = new();

            foreach (EquipmentBase equipment in GetRandomUniqueEquipment(EquipmentChoiceCount)) {
                if (!EquipmentDatabase.TryGetID(equipment, out int equipmentID))
                    continue;

                equipmentChoices.Add(new EquipmentInstanceData(equipmentID, GetRandomQuality()));
            }

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

        private UIItemQuality GetRandomQuality() {
            int totalWeight = 0;

            foreach (UIItemQuality quality in System.Enum.GetValues(typeof(UIItemQuality)))
                totalWeight += GetQualityWeight(quality);

            if (totalWeight <= 0) {
                Debug.LogError("Treasure encounter contains no valid quality weights. Defaulting to Poor.");
                return UIItemQuality.Poor;
            }

            int roll = Random.Range(0, totalWeight);

            foreach (UIItemQuality quality in System.Enum.GetValues(typeof(UIItemQuality))) {
                int weight = GetQualityWeight(quality);

                if (roll < weight)
                    return quality;

                roll -= weight;
            }

            return UIItemQuality.Poor;
        }

        private int GetQualityWeight(UIItemQuality quality) {
            return quality switch {
                UIItemQuality.Poor => PoorWeight,
                UIItemQuality.Common => CommonWeight,
                UIItemQuality.Uncommon => UncommonWeight,
                UIItemQuality.Rare => RareWeight,
                UIItemQuality.Epic => EpicWeight,
                UIItemQuality.Legendary => LegendaryWeight,
                _ => 0
            };
        }
    }
}
