using _Rogues_Path.Pawns;
using _Rogues_Path.Pawns.Scripts;

namespace _Rogues_Path.PawnCommands.Calculators {
    public class Flat: FloatCalculator {
        public int Amount;
        public override float Calculate(Pawn hero) {
            return Amount;
        }
    }
}