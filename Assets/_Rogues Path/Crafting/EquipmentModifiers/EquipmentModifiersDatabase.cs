using System.Collections.Generic;
using UnityEngine;

namespace _Rogues_Path.Crafting {
    [CreateAssetMenu(menuName = "Rogue's Path/Crafting/Equipment Modifier Database", fileName = "EquipmentModifierDatabase")]
    public class EquipmentModifierDatabase : ScriptableObject {
        public List<EquipmentModifierDefinition> Modifiers => modifiers;
        [SerializeField]
        private List<EquipmentModifierDefinition> modifiers = new();

        public bool TryGetByID(int id, out EquipmentModifierDefinition modifier) {
            modifier = null;

            if (id < 0 || id >= Modifiers.Count)
                return false;

            modifier = Modifiers[id];

            return modifier != null;
        }

        public bool TryGetID(EquipmentModifierDefinition modifier, out int id) {
            id = -1;

            if (modifier == null)
                return false;

            id = Modifiers.IndexOf(modifier);

            return id >= 0;
        }
    }
}