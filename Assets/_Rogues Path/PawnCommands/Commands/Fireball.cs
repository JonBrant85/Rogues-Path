using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Rogues_Path.Commands;
using _Rogues_Path.PawnCommands.Calculators;
using _Rogues_Path.Pawns;
using _Rogues_Path.Pawns.Scripts;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Rogues_Path.PawnCommands.Commands {
    public class Fireball : Command {
        public AnimationClip AnimationClip;
        public GameObject FireballPrefab;
        public GameObject ImpactVFX;
        public GameObject DistortionVFX;
        public IntelligenceMultiplier Damage;
        public float TravelTime = 1;
        public Vector3 Offset = new Vector3(1, 1, 0);

        public Fireball(Pawn hero) : base(hero) {}

        public override async UniTask Execute(Pawn instigator, List<Pawn> victims) {
            instigator.Character.AnimationManager.Spring();
            await ShootFireball(instigator, victims.First());
            await UniTask.Delay((int)instigator.PlayAnimation(AnimationClip) * 1000);
        }

        private async UniTask ShootFireball(Pawn caster, Pawn target) {
            var fireball = GameObject.Instantiate(FireballPrefab);
            fireball.transform.position = caster.transform.position + Offset;
            float totalTime = TravelTime;
            float elapsedTime = 0f;


            while (elapsedTime < totalTime) {
                elapsedTime += Time.deltaTime;

                // Calculate percentage completed (always ranges between 0 and 1)
                float t = elapsedTime / totalTime;

                // Update the object's position
                fireball.transform.position = Vector3.Lerp(caster.transform.position + Offset, target.transform.position, t);

                //UniTask.DelayFrame(1); // Wait until the next frame
                await UniTask.Yield();
            }

            // Hard-snap to the exact final position to fix float rounding errors
            fireball.transform.position = target.transform.position;
            GameObject.Destroy(fireball);
            GameObject.Instantiate(ImpactVFX, target.transform.position, Quaternion.identity);
            GameObject.Instantiate(DistortionVFX, target.transform.position, Quaternion.identity);

            await UniTask.Delay((int)target.TakeDamage((int)Damage.Calculate(caster), caster) * 1000);
        }
    }
}