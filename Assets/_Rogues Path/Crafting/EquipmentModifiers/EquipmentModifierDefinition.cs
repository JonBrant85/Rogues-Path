using _Rogues_Path.UI.CharacterScreen;
using Kryz.CharacterStats;
using UnityEngine;


namespace _Rogues_Path.Crafting {
    [CreateAssetMenu(menuName = "Rogue's Path/Crafting/Equipment Modifier", fileName = "New Equipment Modifier")]
    public class EquipmentModifierDefinition : ScriptableObject {
        public string Name;
        public CharacterStatID StatID;
        public StatModType ModifierType = StatModType.Flat;
        public float MinimumValue;
        public float MaximumValue;

        [Min(1)]
        public int Weight = 100;
    }
}