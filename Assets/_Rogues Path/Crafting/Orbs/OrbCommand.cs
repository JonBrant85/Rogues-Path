using System;

namespace _Rogues_Path.Crafting.Commands {
    [Serializable]
    public abstract class OrbCommand : IOrbCommand {
        public abstract bool Execute(OrbCommandContext context);
    }
}