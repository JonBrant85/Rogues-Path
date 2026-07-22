using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;

namespace _Rogues_Path.Pawns.Brains {
    public abstract class PawnBrain : SerializedMonoBehaviour {
        public float ActionDelay = 1;
        public float TimeUntilAction;
        public abstract UniTask HandleTurn();
    }
}