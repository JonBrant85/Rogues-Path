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
        [Min(1)] public int EquipmentRewardCount = 1;
        public string ButtonText = "Open";

        public override async UniTask HandleEncounter(Transform windowContent, Transform bottomBar, Button buttonPrefab) {
            if (!await WaitForConfirmation(bottomBar, buttonPrefab, ButtonText))
                return;

            int equipmentGranted = 0;

            foreach (EquipmentBase equipment in EquipmentDatabase.GetRandomEquipment(EquipmentRewardCount)) {
                if (equipment == null || !EquipmentDatabase.TryGetID(equipment, out int equipmentID))
                    continue;

                Game.Instance.PlayerInventory.Add(new EquipmentInstanceData(equipmentID));
                equipmentGranted++;
            }

            if (equipmentGranted > 0)
                EventBus.Raise(new InventoryChanged());

            Debug.Log($"Treasure encounter granted {equipmentGranted} equipment item(s).");
        }
    }
}
