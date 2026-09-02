using System;
using System.Collections.Generic;
using DuloGames.UI;

namespace _Rogues_Path.Crafting {
    [Serializable]
    public class EquipmentInstanceData {
        public int EquipmentID;
        public UIItemQuality Quality;
        public List<RolledEquipmentModifier> CraftedModifiers = new();

        public EquipmentInstanceData(int equipmentID, UIItemQuality quality = UIItemQuality.Poor) {
            EquipmentID = equipmentID;
            Quality = quality;
        }
    }
}
