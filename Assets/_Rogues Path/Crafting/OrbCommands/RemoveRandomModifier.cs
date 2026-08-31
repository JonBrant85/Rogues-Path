using System;

namespace _Rogues_Path.Crafting.Commands {
    [Serializable]
    public class RemoveRandomModifier : OrbCommand {
        public override bool Execute(
            OrbCommandContext context) {

            if (context?.Equipment?.CraftedModifiers == null ||
                context.Equipment.CraftedModifiers.Count == 0) {

                return false;
            }

            int index = UnityEngine.Random.Range(
                0,
                context.Equipment.CraftedModifiers.Count);

            context.Equipment.CraftedModifiers.RemoveAt(index);

            return true;
        }
    }
}