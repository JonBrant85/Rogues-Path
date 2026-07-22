using _Rogues_Path.Pawns;

namespace _Rogues_Path.PawnCommands.Calculators {
    public class Flat: IntCalculator {
        public int Amount;
        public override int Calculate(Pawn hero) {
            return Amount;
        }
    }
}