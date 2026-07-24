using System;
using _Rogues_Path.Pawns;

namespace _Rogues_Path.PawnCommands.Calculators {
    [Serializable]
    public  class IntCalculator {
        public virtual int Calculate(Pawn hero){return Int32.MinValue;}
    }
}