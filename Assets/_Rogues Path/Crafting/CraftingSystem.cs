using System.Collections.Generic;
using _Rogues_Path.Equipment.Scripts;
using Kryz.CharacterStats;
using UnityEngine;

namespace _Rogues_Path.Crafting {
    public static class CraftingSystem {
        public static bool TryAddRandomModifier(EquipmentBase equipment, EquipmentModifierDatabase modifierDatabase) {

            if (equipment == null || modifierDatabase == null)
                return false;

            if (EquipmentDatabase.IsDatabaseEntry(equipment)) {
                Debug.LogError($"Cannot craft database equipment {equipment.Name}. " + $"Crafting requires a live equipment instance.");

                return false;
            }

            EquipmentModifierDefinition modifierDefinition = GetRandomModifier(modifierDatabase.Modifiers);

            if (modifierDefinition == null)
                return false;

            float value = Random.Range(modifierDefinition.MinimumValue, modifierDefinition.MaximumValue);

            StatAndModifierPair modifier = new() {
                StatID = modifierDefinition.StatID,
                Modifier = new StatModifier(value, modifierDefinition.ModifierType)
            };

            equipment.Modifiers.Add(modifier);

            if (equipment.Owner != null) {
                equipment.ApplyModifiers(
                    new List<StatAndModifierPair> {
                        modifier
                    },
                    equipment.Owner);
            }

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

            int roll = UnityEngine.Random.Range(0, totalWeight);

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