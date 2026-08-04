using System;
using _Rogues_Path.Pawns;
using _Rogues_Path.UI.CharacterScreen;
using UnityEngine;

namespace _Rogues_Path.PawnCommands.Calculators {
    [Serializable]
    public class StrengthMultiplier: FloatCalculator {
        public CharacterStatID ID;
        public float Multiplier = 1;
        public override float Calculate(Pawn hero) {
            Debug.Assert(hero!=null, "Hero == null");
            Debug.Assert(ID != null, "ID null");
            return Multiplier * hero.Stats[ID].Value;
        }
    }
}