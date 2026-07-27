using System;
using _Rogues_Path.Pawns;
using UnityEngine;

namespace _Rogues_Path.PawnCommands.Calculators {
    [Serializable]
    public class StrengthMultiplier: FloatCalculator {
        public float Multiplier = 1;
        public override float Calculate(Pawn hero) {
            Debug.Assert(hero!=null, "Hero == null");
            return Multiplier * hero.Strength.Value;
        }
    }
}