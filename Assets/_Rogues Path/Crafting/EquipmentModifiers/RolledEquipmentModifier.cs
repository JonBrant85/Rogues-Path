using System;

namespace _Rogues_Path.Crafting {
    [Serializable]
    public class RolledEquipmentModifier {
        public int ModifierID;
        public float Value;

        public RolledEquipmentModifier(int modifierID, float value) {
            ModifierID = modifierID;
            Value = value;
        }
    }
}