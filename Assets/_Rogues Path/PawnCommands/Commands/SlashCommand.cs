using System.Collections.Generic;
using System.Linq;
using _Rogues_Path.Commands;
using _Rogues_Path.Pawns;
using Cysharp.Threading.Tasks;

namespace _Rogues_Path.PawnCommands.Commands {
    public class SlashCommand : Command{
        public int damage;
        public SlashCommand(Pawn hero) : base(hero) {}

        public override async UniTask Execute(Pawn instigator, List<Pawn> victims) {
            instigator.Slash();
            await UniTask.Delay((int)instigator.Jab()*1000);
            await UniTask.Delay((int)victims.FirstOrDefault().TakeDamage(damage, instigator)*1000);
        }
    }
}