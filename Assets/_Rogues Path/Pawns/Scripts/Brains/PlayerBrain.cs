using Cysharp.Threading.Tasks;

namespace _Rogues_Path.Pawns.Brains {
    public class PlayerBrain: PawnBrain {
        public async override UniTask HandleTurn() {
            await UniTask.Delay(1000);
        }
    }
}