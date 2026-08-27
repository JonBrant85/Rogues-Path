using System.Linq;
using _Rogues_Path.Crafting;
using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.Pawns.Scripts;
using Sirenix.OdinInspector;
using UnityEngine;

public class CraftingDebug : MonoBehaviour {
    [SerializeField] private Pawn pawn;
    [SerializeField] private EquipmentModifierDatabase modifierDatabase;

    [Button]
    public static bool TryAddRandomModifier(EquipmentInstanceData equipment, EquipmentModifierDatabase modifierDatabase) {

        if (equipment == null || modifierDatabase == null)
            return false;

        EquipmentModifierDefinition modifierDefinition = CraftingSystem.GetRandomModifier(modifierDatabase.Modifiers);

        if (modifierDefinition == null)
            return false;

        if (!modifierDatabase.TryGetID(modifierDefinition, out int modifierID))
            return false;

        float value = UnityEngine.Random.Range(modifierDefinition.MinimumValue, modifierDefinition.MaximumValue);

        equipment.CraftedModifiers.Add(new RolledEquipmentModifier(modifierID, value));

        return true;
    }
}