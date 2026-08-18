using System.Collections.Generic;
using _Rogues_Path.Commands;
using _Rogues_Path.Pawns;
using _Rogues_Path.Pawns.Scripts;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Rogues_Path.PawnCommands.Commands {
    public class ApplyDebuff : Command{
        public AnimationClip AnimationClip;
        public PawnBuffs.Buffs.Block Debuff;
        public int Count;
        public ApplyDebuff(Pawn hero) : base(hero) {}

        public override async UniTask Execute(Pawn instigator, List<Pawn> victims) {
            await UniTask.Delay((int)instigator.PlayAnimation(AnimationClip)*1000);
            victims.ForEach(pawn => pawn.AddBuff(Debuff, Count));
        }
    }
}