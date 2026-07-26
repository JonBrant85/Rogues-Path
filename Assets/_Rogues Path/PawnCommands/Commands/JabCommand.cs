using System.Collections.Generic;
using System.Linq;
using _Rogues_Path.Commands;
using _Rogues_Path.PawnCommands.Calculators;
using _Rogues_Path.Pawns;
using Cysharp.Threading.Tasks;

namespace _Rogues_Path.PawnCommands.Commands {
    public class JabCommand : Command{
        public StrengthMultiplier damage;
        public JabCommand(Pawn hero) : base(hero) {}

        public override async UniTask Execute(Pawn instigator, List<Pawn> victims) {
            await UniTask.Delay((int)instigator.Jab()*1000);
            await UniTask.Delay((int)victims.FirstOrDefault().TakeDamage(damage.Calculate(instigator), instigator)*1000);
        }
    }
}