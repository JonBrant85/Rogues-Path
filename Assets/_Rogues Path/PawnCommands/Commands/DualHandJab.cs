using System;
using System.Collections.Generic;
using System.Linq;
using _Rogues_Path.Commands;
using _Rogues_Path.PawnCommands.Calculators;
using _Rogues_Path.Pawns;
using _Rogues_Path.Pawns.Scripts;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Rogues_Path.PawnCommands.Commands {
    public class DualHandJab : Command {
        public AnimationClip AnimationClip;
        public DexterityCalculator DexterityMultiplier;
        public DualHandJab(Pawn hero) : base(hero) {}

        public override async UniTask Execute(Pawn instigator, List<Pawn> victims) {
            instigator.PlayAnimation(AnimationClip);
            await UniTask.Delay((int)(AnimationClip.length * 1000));
            await UniTask.Delay((int)victims.FirstOrDefault()!.TakeDamage((int)DexterityMultiplier.Calculate(instigator), instigator) * 1000);
        }
    }
}