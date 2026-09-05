using _Rogues_Path.Pawns.Scripts;

namespace _Rogues_Path.Utilities.Events {
    public struct DamageApplied : IEvent {
        public Pawn Victim;
        public Pawn Instigator;
        public float Amount;
    }
}
