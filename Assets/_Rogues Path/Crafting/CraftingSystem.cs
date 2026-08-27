using System.Collections.Generic;
using UnityEngine;

namespace _Rogues_Path.Crafting {
    public static class CraftingSystem {
        public static bool TryAddRandomModifier(EquipmentInstanceData equipment, EquipmentModifierDatabase modifierDatabase) {

            if (equipment == null || modifierDatabase == null)
                return false;

            EquipmentModifierDefinition modifierDefinition = GetRandomModifier(modifierDatabase.Modifiers);

            if (modifierDefinition == null)
                return false;

            if (!modifierDatabase.TryGetID(modifierDefinition, out int modifierID)) {

                return false;
            }

            float value = Random.Range(modifierDefinition.MinimumValue, modifierDefinition.MaximumValue);

            equipment.CraftedModifiers.Add(new RolledEquipmentModifier(modifierID, value));

            return true;
        }

        public static EquipmentModifierDefinition GetRandomModifier(IReadOnlyList<EquipmentModifierDefinition> modifiers) {

            if (modifiers == null || modifiers.Count == 0)
                return null;

            int totalWeight = 0;

            foreach (EquipmentModifierDefinition modifier in modifiers) {
                if (modifier == null)
                    continue;

                totalWeight += modifier.Weight;
            }

            if (totalWeight <= 0)
                return null;

            int roll = Random.Range(0, totalWeight);

            foreach (EquipmentModifierDefinition modifier in modifiers) {
                if (modifier == null)
                    continue;

                if (roll < modifier.Weight)
                    return modifier;

                roll -= modifier.Weight;
            }

            return null;
        }
    }
}