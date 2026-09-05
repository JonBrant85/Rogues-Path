using _Rogues_Path.Pawns.Scripts;

namespace _Rogues_Path.Utilities.Events {
    public struct RunPawnsChanged : IEvent {
        public Pawn Player;
        public Pawn Enemy;
    }
}
