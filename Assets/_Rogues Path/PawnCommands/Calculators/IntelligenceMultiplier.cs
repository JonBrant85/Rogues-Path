using System;
using _Rogues_Path.Pawns;

namespace _Rogues_Path.PawnCommands.Calculators {
    [Serializable]
    public class IntelligenceMultiplier : FloatCalculator {
        public float Multiplier = 1;
        public override float Calculate(Pawn hero) {
            return Multiplier * hero.Intelligence.Value;
        }
    }
}