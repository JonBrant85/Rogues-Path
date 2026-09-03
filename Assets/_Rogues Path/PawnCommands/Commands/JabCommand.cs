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
            Animator animator = instigator.animationManager.Animator;
            int upperLayer = animator.GetLayerIndex("Upper");

            Debug.Log(
                $"JAB ANIMATION | Before | Animator={animator.name} | "
                + $"Enabled={animator.enabled} | "
                + $"Controller={animator.runtimeAnimatorController?.name ?? "NULL"} | "
                + $"UpperLayer={upperLayer} | "
                + $"Action={animator.GetBool("Action")}");

            instigator.animationManager.Jab();
            LogAnimatorAfterTransition(animator, upperLayer).Forget();
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            if (upperLayer < 0) {
                Debug.LogError("JAB ANIMATION | Animator has no Upper layer.");
            }
            else {
                bool inTransition = animator.IsInTransition(upperLayer);
                AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(upperLayer);
                AnimatorStateInfo nextState = inTransition
                    ? animator.GetNextAnimatorStateInfo(upperLayer)
                    : default;

                AnimatorClipInfo[] currentClips =
                    animator.GetCurrentAnimatorClipInfo(upperLayer);

                AnimatorClipInfo[] nextClips = inTransition
                    ? animator.GetNextAnimatorClipInfo(upperLayer)
                    : System.Array.Empty<AnimatorClipInfo>();

                string currentClip =
                    currentClips.Length > 0 && currentClips[0].clip != null
                        ? currentClips[0].clip.name
                        : "NONE";

                string nextClip =
                    nextClips.Length > 0 && nextClips[0].clip != null
                        ? nextClips[0].clip.name
                        : "NONE";

                int jabHash = Animator.StringToHash("Jab");

                Debug.Log(
                    $"JAB ANIMATION | After | CurrentClip={currentClip} | "
                    + $"CurrentIsJab={currentState.shortNameHash == jabHash} | "
                    + $"InTransition={inTransition} | "
                    + $"NextClip={nextClip} | "
                    + $"NextIsJab={inTransition && nextState.shortNameHash == jabHash} | "
                    + $"Action={animator.GetBool("Action")}");
            }

            await UniTask.Delay((int)AnimationClip.length * 1000);
            await UniTask.Delay((int)victims.FirstOrDefault()!.TakeDamage((int)StrengthMultiplier.Calculate(instigator), instigator) * 1000);
        }

        private static async UniTask LogAnimatorAfterTransition(
            Animator animator,
            int upperLayer) {

            await UniTask.Delay(150);

            if (animator == null || upperLayer < 0)
                return;

            bool inTransition = animator.IsInTransition(upperLayer);
            AnimatorStateInfo currentState =
                animator.GetCurrentAnimatorStateInfo(upperLayer);

            AnimatorClipInfo[] currentClips =
                animator.GetCurrentAnimatorClipInfo(upperLayer);

            string currentClip =
                currentClips.Length > 0 && currentClips[0].clip != null
                    ? currentClips[0].clip.name
                    : "NONE";

            Debug.Log(
                $"JAB ANIMATION | Midpoint | CurrentClip={currentClip} | "
                + $"CurrentIsJab={currentState.shortNameHash == Animator.StringToHash("Jab")} | "
                + $"NormalizedTime={currentState.normalizedTime:F2} | "
                + $"InTransition={inTransition} | "
                + $"UpperWeight={animator.GetLayerWeight(upperLayer):F2} | "
                + $"AnimatorSpeed={animator.speed:F2} | "
                + $"ClipSpeed={animator.GetFloat("Speed"):F2} | "
                + $"Action={animator.GetBool("Action")}");
        }
    }
}
