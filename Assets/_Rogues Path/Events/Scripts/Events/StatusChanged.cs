using System.Collections.Generic;
using _Rogues_Path.Buffs.Scripts;
using _Rogues_Path.Pawns;
using _Rogues_Path.Pawns.Scripts;

namespace _Rogues_Path.Utilities.Events {
    public struct StatusChanged: IEvent {
        public List<Pawn> Targets;
        public PawnBuff NewStatus;
        public int Count;
    }
}