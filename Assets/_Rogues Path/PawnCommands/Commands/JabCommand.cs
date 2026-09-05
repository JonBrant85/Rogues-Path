using System.Collections.Generic;
using System.Linq;
using _Rogues_Path.Commands;
using _Rogues_Path.PawnCommands.Calculators;
using _Rogues_Path.Pawns;
using _Rogues_Path.Pawns.Scripts;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Rogues_Path.PawnCommands.Commands {
    public class JabCommand : Command {
        public AnimationClip AnimationClip;
        public StrengthMultiplier StrengthMultiplier;
        public JabCommand(Pawn hero) : base(hero) {}

        public override async UniTask Execute(Pawn instigator, List<Pawn> victims) {
            instigator.animationManager.Jab();
            await UniTask.Delay(Mathf.RoundToInt(AnimationClip.length * 1000f));
            await UniTask.Delay((int)victims.FirstOrDefault()!.TakeDamage((int)StrengthMultiplier.Calculate(instigator), instigator) * 1000);
        }
    }
}