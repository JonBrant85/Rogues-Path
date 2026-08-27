using System;
using System.Collections.Generic;

namespace _Rogues_Path.Crafting {
    [Serializable]
    public class EquipmentInstanceData {
        public int EquipmentID;
        public List<RolledEquipmentModifier> CraftedModifiers = new();

        public EquipmentInstanceData(int equipmentID) {
            EquipmentID = equipmentID;
        }
    }
}