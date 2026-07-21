using System.Collections.Generic;
using _Rogues_Path.Pawns;

namespace _Rogues_Path.Commands {
    public struct CommandContext {
        public Pawn Caster;
        public List<Pawn> Targets;

        public CommandContext(Pawn caster, List<Pawn> targets) {
            Caster = caster;
            Targets = targets;
        }
    }
}