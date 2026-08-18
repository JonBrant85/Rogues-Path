using System;
using _Rogues_Path.Pawns;
using _Rogues_Path.Pawns.Scripts;

namespace _Rogues_Path.PawnCommands.Calculators {
    [Serializable]
    public  class FloatCalculator {
        public virtual float Calculate(Pawn hero){return Single.MaxValue;}
    }
}