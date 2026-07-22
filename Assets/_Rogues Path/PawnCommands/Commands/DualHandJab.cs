using System.Collections.Generic;
using System.Linq;
using _Rogues_Path.Commands;
using _Rogues_Path.Pawns;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Rogues_Path.PawnCommands.Commands {
    public class DualHandJab : Command {
        public AnimationClip AnimationClip;
        public int damage;
        public DualHandJab(Pawn hero) : base(hero) {}

        public override async UniTask Execute(Pawn instigator, List<Pawn> victims) {
            //instigator.PlayAnimation(AnimationClip);
            await UniTask.Delay((int)instigator.PlayAnimation(AnimationClip)*1000);
            await UniTask.Delay((int)victims.FirstOrDefault().TakeDamage(damage, instigator)*1000);
        }
    }
}