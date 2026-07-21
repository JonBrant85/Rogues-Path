using System.Collections.Generic;
using System.Linq;
using _Rogues_Path.Commands;
using _Rogues_Path.Pawns;
using Cysharp.Threading.Tasks;

namespace _Rogues_Path.PawnCommands.Commands {
    public class JabCommand : Command{
        public int damage;
        public JabCommand(Pawn hero) : base(hero) {}

        public override async UniTask Execute(Pawn instigator, List<Pawn> victims) {
            instigator.Slash();
            await UniTask.WaitForSeconds(instigator.Jab());
            await UniTask.WaitForSeconds(victims.FirstOrDefault().TakeDamage(damage, instigator));
        }
    }
}