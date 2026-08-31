using System;
using UnityEngine;

namespace _Rogues_Path.Crafting.Commands {
    [Serializable]
    public class AddRandomModifiers : OrbCommand {
        [Min(1)]
        public int Amount = 1;

        public override bool Execute(OrbCommandContext context) {

            if (context?.Equipment == null || context.ModifierDatabase == null) {

                return false;
            }

            for (int i = 0; i < Amount; i++) {
                if (!CraftingSystem.TryAddRandomModifier(context.Equipment, context.ModifierDatabase)) {

                    return false;
                }
            }

            return true;
        }
    }
}