using System.Collections.Generic;
using _Rogues_Path.Buffs.Scripts;
using _Rogues_Path.Pawns;

namespace _Rogues_Path.Utilities.Events {
    public struct StatusChangedEvent: IEvent {
        public List<Pawn> Targets;
        public PawnBuff NewStatus;
        public int Count;
    }
}