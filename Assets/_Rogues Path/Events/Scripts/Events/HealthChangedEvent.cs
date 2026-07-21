using _Rogues_Path.Pawns;

namespace _Rogues_Path.Utilities.Events {
    public struct HealthChangedEvent : IEvent {
        public Pawn Victim;
        public Pawn Instigator;
        public float NewHealth;
        public float OldHealth;
    }
}