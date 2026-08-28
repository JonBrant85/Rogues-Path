namespace _Rogues_Path.Crafting.Commands {
    public class OrbCommandContext {
        public EquipmentInstanceData Equipment;
        public EquipmentModifierDatabase ModifierDatabase;

        public OrbCommandContext(EquipmentInstanceData equipment, EquipmentModifierDatabase modifierDatabase) {

            Equipment = equipment;
            ModifierDatabase = modifierDatabase;
        }
    }
}