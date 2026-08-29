using System.Collections.Generic;
using System.Linq;
using _Rogues_Path.Commands;
using _Rogues_Path.PawnCommands.Calculators;
using _Rogues_Path.Pawns;
using _Rogues_Path.Pawns.Scripts;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Rogues_Path.PawnCommands.Commands {
    public class BowAttack : Command {
        public AnimationClip BowShotAnimation;
        public DexterityCalculator DexterityMultiplier;

        public BowAttack(Pawn hero) : base(hero) {}

        public async override UniTask Execute(Pawn instigator, List<Pawn> victims) {
            await UniTask.Delay((int)(instigator.PlayAnimation(BowShotAnimation) * 1000));
            await UniTask.Delay((int)(victims.First().TakeDamage((int)DexterityMultiplier.Calculate(instigator), instigator) * 1000));
        }
    }
}