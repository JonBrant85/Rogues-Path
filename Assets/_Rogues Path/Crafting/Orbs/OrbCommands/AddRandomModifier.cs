using System;

namespace _Rogues_Path.Crafting.Commands {
    [Serializable]
    public class AddRandomModifier : OrbCommand {
        public override bool Execute(OrbCommandContext context) {
            if (context == null)
                return false;

            if (context.Equipment == null)
                return false;

            if (context.ModifierDatabase == null)
                return false;

            return CraftingSystem.TryAddRandomModifier(context.Equipment, context.ModifierDatabase);
        }
    }
}