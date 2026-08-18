using System.Collections.Generic;
using System.Linq;
using _Rogues_Path.Commands;
using _Rogues_Path.PawnCommands.Calculators;
using _Rogues_Path.Pawns;
using _Rogues_Path.Pawns.Scripts;
using Cysharp.Threading.Tasks;

namespace _Rogues_Path.PawnCommands.Commands {
    public class SlashCommand : Command {
        public StrengthMultiplier StrengthMultiplier;
        public SlashCommand(Pawn hero) : base(hero) {}

        public override async UniTask Execute(Pawn instigator, List<Pawn> victims) {
            instigator.Slash();
            await UniTask.Delay((int)instigator.Jab() * 1000);
            await UniTask.Delay((int)victims.FirstOrDefault()!.TakeDamage((int)StrengthMultiplier.Calculate(instigator), instigator) * 1000);
        }
    }
}