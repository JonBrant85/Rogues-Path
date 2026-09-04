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
            Transform activeCharacter = instigator.Character.Active != null
                ? instigator.Character.Active.transform
                : null;

            Transform upperBody = activeCharacter?.Find("UpperBody");
            Transform leftArm = activeCharacter?.Find("UpperBody/ArmLAnchor/ArmL");
            Transform rightArm = activeCharacter?.Find("UpperBody/ArmRAnchor/ArmR");

            Quaternion upperBodyBefore = upperBody != null
                ? upperBody.localRotation
                : Quaternion.identity;

            Quaternion leftArmBefore = leftArm != null
                ? leftArm.localRotation
                : Quaternion.identity;

            Quaternion rightArmBefore = rightArm != null
                ? rightArm.localRotation
                : Quaternion.identity;

            instigator.animationManager.Jab();

            await UniTask.Delay(Mathf.RoundToInt(AnimationClip.length * 1000f));
            await UniTask.Delay((int)victims.FirstOrDefault()!.TakeDamage((int)StrengthMultiplier.Calculate(instigator), instigator) * 1000);
        }

        private static float GetRotationDelta(
            Transform target,
            Quaternion before) {

            return target != null
                ? Quaternion.Angle(before, target.localRotation)
                : -1f;
        }
    }
}
