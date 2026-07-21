using _Rogues_Path.Pawns;

namespace _Rogues_Path.Utilities.Events {
    public struct PreMitigationDamageReceivedEvent : IEvent {
        public int UnmitigatedDamage;
        public Pawn Victim;
        public Pawn Instigator;
    }
}