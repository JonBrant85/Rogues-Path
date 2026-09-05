using System.Collections.Generic;
using Assets.FantasyMonsters.Common.Scripts;
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
        public string ContinueButtonText = "Continue";
        [TextArea] public string SuccessResultText = "Inside, you find {item} ({quality}). You secure it among your belongings.";
        [TextArea] public string EmptyResultText = "The chest opens with a hollow creak. Whatever it once held is long gone.";
        [Min(0)] public int PoorWeight = 10;
        [Min(0)] public int CommonWeight = 40;
        [Min(0)] public int UncommonWeight = 25;
        [Min(0)] public int RareWeight = 15;
        [Min(0)] public int EpicWeight = 7;
        [Min(0)] public int LegendaryWeight = 3;

        public override async UniTask HandleEncounter(Transform windowContent, Transform bottomBar, Button buttonPrefab) {
            List<EquipmentInstanceData> equipmentChoices = new();

            foreach (EquipmentBase query in GetRandomUniqueEquipment(EquipmentChoiceCount)) {
                if (!EquipmentDatabase.TryGetID(query, out int equipmentID))
                    continue;

                equipmentChoices.Add(new EquipmentInstanceData(equipmentID, GetRandomQuality()));
            }

            if (equipmentChoices.Count == 0) {
                Debug.LogError("Treasure encounter could not find any equipment choices.");
                await ShowEmptyResult();
                return;
            }

            UIEncounterWindow encounterWindow = UIEncounterWindow.Instance;

            if (encounterWindow == null) {
                Debug.LogError("Treasure encounter could not find UIEncounterWindow.");
                return;
            }

            EquipmentInstanceData selectedEquipment = await encounterWindow.WaitForEquipmentSelection(
                equipmentChoices,
                ButtonText);

            if (encounterWindow == null)
                return;

            if (selectedEquipment == null
                || !EquipmentDatabase.TryGetByID(selectedEquipment.EquipmentID, out EquipmentBase equipment)) {
                Debug.LogError("Treasure encounter could not resolve a valid equipment selection.");
                await ShowEmptyResult();
                return;
            }

            Game.Instance.PlayerInventory.Add(selectedEquipment);
            EventBus.Raise(new TreasureClaimed { Equipment = selectedEquipment });
            EventBus.Raise(new InventoryChanged());
            OpenChest();

            string resultText = SuccessResultText
                .Replace("{item}", equipment.Name)
                .Replace("{quality}", selectedEquipment.Quality.ToString());

            Debug.Log($"Treasure encounter granted {equipment.Name}.");
            await encounterWindow.ShowResult(resultText, ContinueButtonText);
        }

        private async UniTask ShowEmptyResult() {
            OpenChest();
            UIEncounterWindow encounterWindow = UIEncounterWindow.Instance;

            if (encounterWindow != null)
                await encounterWindow.ShowResult(EmptyResultText, ContinueButtonText);
        }

        private void OpenChest() {
            if (RuntimeWorldVisual == null) {
                Debug.LogWarning($"Treasure encounter '{name}' has no runtime chest visual to open.");
                return;
            }

            Monster chest = RuntimeWorldVisual.GetComponent<Monster>();

            if (chest == null) {
                Debug.LogWarning($"Treasure encounter '{name}' found no Monster animation component on its chest visual.");
                return;
            }

            chest.Die();
        }

        private static List<EquipmentBase> GetRandomUniqueEquipment(int count) {
            List<EquipmentBase> availableEquipment = EquipmentDatabase.Instance != null ? EquipmentDatabase.Instance.Equipment : new List<EquipmentBase>();

            availableEquipment.RemoveAll(equipment => equipment == null);

            int choiceCount = Mathf.Min(count, availableEquipment.Count);

            for (int i = 0; i < choiceCount; i++) {
                int randomIndex = Random.Range(i, availableEquipment.Count);
                (availableEquipment[i], availableEquipment[randomIndex]) = (availableEquipment[randomIndex], availableEquipment[i]);
            }

            if (choiceCount < availableEquipment.Count)
                availableEquipment.RemoveRange(choiceCount, availableEquipment.Count - choiceCount);

            return availableEquipment;
        }

        private UIItemQuality GetRandomQuality() {
            return EquipmentQualityRoller.Roll(
                PoorWeight,
                CommonWeight,
                UncommonWeight,
                RareWeight,
                EpicWeight,
                LegendaryWeight);
        }
    }
}
