using System.Collections.Generic;
using _Rogues_Path.Commands;
using _Rogues_Path.PawnCommands.Calculators;
using _Rogues_Path.Pawns;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Rogues_Path.PawnCommands.Commands {
    public class HealCommand : Command {
        public bool TargetCaster = false;
        public float AnimationDuration = 1;
        public IntelligenceMultiplier DamageToHeal;
        public HealCommand(Pawn hero) : base(hero) {}

        public async override UniTask Execute(Pawn instigator, List<Pawn> victims) {
            if (TargetCaster) {
                await UniTask.Delay((int)AnimationDuration*1000);
                Debug.Log($"Healing caster for {DamageToHeal.Calculate(instigator)}");
                await UniTask.Delay((int)instigator.TakeDamage(DamageToHeal.Calculate(instigator)*1000, instigator));
            }
            else {
                foreach (var target in victims) {
                    await UniTask.Delay((int)target.TakeDamage(DamageToHeal.Calculate(target)*1000, target));
                }
            }
        }
    }
}