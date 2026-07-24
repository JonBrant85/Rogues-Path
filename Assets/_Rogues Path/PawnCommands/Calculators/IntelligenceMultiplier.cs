using System;
using _Rogues_Path.Pawns;

namespace _Rogues_Path.PawnCommands.Calculators {
    [Serializable]
    public class IntelligenceMultiplier : IntCalculator {
        public int Multiplier = 1;
        public override int Calculate(Pawn hero) {
            return (int)(Multiplier * hero.Intelligence.Value);
        }
    }
}