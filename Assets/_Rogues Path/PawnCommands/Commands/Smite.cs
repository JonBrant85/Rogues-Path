using System.Collections.Generic;
using _Rogues_Path.Commands;
using _Rogues_Path.PawnCommands.Calculators;
using _Rogues_Path.Pawns.Scripts;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Rogues_Path.PawnCommands.Commands {
    public class Smite: Command {
        public IntelligenceMultiplier Damage;
        public GameObject VFX;
        public float VFXLifetime = 5;
        
        public Smite(Pawn hero) : base(hero) {}
        public async override UniTask Execute(Pawn instigator, List<Pawn> victims) {
            foreach (var victim in victims) {
                var vfx = GameObject.Instantiate(VFX, victim.transform.position, Quaternion.identity);
                GameObject.Destroy(vfx, VFXLifetime);
                await UniTask.Delay((int)victim.TakeDamage((int)Damage.Calculate(instigator), instigator) * 1000);
            }
        }
    }
}