using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DuloGames.UI;
using Sirenix.OdinInspector;

namespace _Rogues_Path.Pawns.Brains {
    public abstract class PawnBrain : SerializedMonoBehaviour {
        public List<UISpellInfo> Spells = new();
        public Pawn Owner;
        public float ActionDelay = 1;
        public float TimeUntilAction;
        public abstract UniTask HandleTurn();
    }
}