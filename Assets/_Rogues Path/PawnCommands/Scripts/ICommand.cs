using System.Collections.Generic;
using _Rogues_Path.Pawns;
using Cysharp.Threading.Tasks;

namespace _Rogues_Path.Commands {
    public interface ICommand {
        UniTask Execute(Pawn instigator, List<Pawn> victims);
    }
}