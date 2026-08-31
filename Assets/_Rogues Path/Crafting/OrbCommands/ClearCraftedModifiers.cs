using System;

namespace _Rogues_Path.Crafting.Commands {
    [Serializable]
    public class ClearCraftedModifiers : OrbCommand {
        public override bool Execute(OrbCommandContext context) {
            if (context?.Equipment == null)
                return false;

            context.Equipment.CraftedModifiers.Clear();
            return true;
        }
    }
}