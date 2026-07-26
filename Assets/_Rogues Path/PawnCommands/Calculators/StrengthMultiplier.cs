using System;
using _Rogues_Path.Pawns;
using UnityEngine;

namespace _Rogues_Path.PawnCommands.Calculators {
    [Serializable]
    public class StrengthMultiplier: IntCalculator {
        public int Multiplier = 1;
        public override int Calculate(Pawn hero) {
            Debug.Assert(hero!=null, "Hero == null");
            return (int)(Multiplier * hero.Strength.Value);
        }
    }
}