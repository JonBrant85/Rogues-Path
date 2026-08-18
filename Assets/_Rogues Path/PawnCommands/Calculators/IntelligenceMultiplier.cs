using System;
using _Rogues_Path.Pawns;
using _Rogues_Path.Pawns.Scripts;
using _Rogues_Path.UI.CharacterScreen;

namespace _Rogues_Path.PawnCommands.Calculators {
    [Serializable]
    public class IntelligenceMultiplier : FloatCalculator {
        public CharacterStatID ID;
        public float Multiplier = 1;
        public override float Calculate(Pawn hero) {
            return Multiplier * hero.Stats[ID].Value;
        }
    }
}