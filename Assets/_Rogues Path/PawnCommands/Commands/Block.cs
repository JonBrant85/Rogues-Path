using System.Collections.Generic;
using System.Linq;
using _Rogues_Path.Commands;
using _Rogues_Path.Pawns;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Rogues_Path.PawnCommands.Commands {
    public class Block : Command{
        public AnimationClip AnimationClip;
        public PawnBuffs.Buffs.Block BlockBuff;
        public int Count;
        public Block(Pawn hero) : base(hero) {}

        public override async UniTask Execute(Pawn instigator, List<Pawn> victims) {
            instigator.AddBuff(BlockBuff, Count);
            await UniTask.Delay((int)instigator.PlayAnimation(AnimationClip)*1000);
        }
    }
}