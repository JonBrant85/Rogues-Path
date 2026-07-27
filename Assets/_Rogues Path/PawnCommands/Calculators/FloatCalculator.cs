using System;
using _Rogues_Path.Pawns;

namespace _Rogues_Path.PawnCommands.Calculators {
    [Serializable]
    public  class FloatCalculator {
        public virtual float Calculate(Pawn hero){return Single.MaxValue;}
    }
}