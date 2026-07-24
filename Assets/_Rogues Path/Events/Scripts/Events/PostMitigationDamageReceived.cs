using System.Collections.Generic;
using _Rogues_Path.Pawns;

namespace _Rogues_Path.Utilities.Events {
    public class PostMitigationDamageReceived: IEvent {
        public int MitigatedDamage;
        public List<Pawn> Victims;
        public Pawn Instigator;
    }
}