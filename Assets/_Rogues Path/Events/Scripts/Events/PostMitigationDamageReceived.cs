using System.Collections.Generic;
using _Rogues_Path.Pawns;
using _Rogues_Path.Pawns.Scripts;

namespace _Rogues_Path.Utilities.Events {
    public class PostMitigationDamageReceived: IEvent {
        public int MitigatedDamage;
        public List<Pawn> Victims;
        public Pawn Instigator;
    }
}