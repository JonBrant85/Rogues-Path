using _Rogues_Path.Pawns;
using _Rogues_Path.Pawns.Scripts;

namespace _Rogues_Path.Utilities.Events {
    public struct HealthChanged : IEvent {
        public Pawn Victim;
        public Pawn Instigator;
        public float NewHealth;
        public float OldHealth;
    }
}