using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DuloGames.UI;
using Sirenix.OdinInspector;

namespace _Rogues_Path.Pawns.Scripts.Brains {
    public abstract class PawnBrain : SerializedMonoBehaviour {
        public List<UISpellInfo> KnownSpells = new();
        public List<UISpellInfo> PreparedSpells = new();
        public Pawn Owner;
        public float ActionDelay = 1;
        public float TimeUntilAction;
        public abstract UniTask HandleTurn();
    }
}