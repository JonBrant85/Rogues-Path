using _Rogues_Path.Pawns;
using _Rogues_Path.Pawns.Scripts;

namespace _Rogues_Path.Utilities.Events {
    public struct PreMitigationDamageReceived : IEvent {
        public int UnmitigatedDamage;
        public Pawn Victim;
        public Pawn Instigator;
    }
}