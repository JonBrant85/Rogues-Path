using System;
using System.Collections.Generic;
using DG.Tweening;
using OldOdin;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Rogues_Path.Pawns {
    public partial class Pawn {
        [FoldoutGroup("References")] public Animazing Animazing;
        [FoldoutGroup("Damage Text Offset")] [SerializeField] private Vector3 DamageOffset = new Vector3(-1, 0, 0);
        [FoldoutGroup("Damage Text Offset")] [SerializeField] private Vector3 HealingOffset = new Vector3(1, 0, 0);

        [FoldoutGroup("Animations"), SerializeField] private AnimationClip IdleAnimation;
        [FoldoutGroup("Animations"), SerializeField] private AnimationClip SlashAnimation;
        [FoldoutGroup("Animations"), SerializeField] private AnimationClip ChargeAttackAnimation;
        [FoldoutGroup("Animations"), SerializeField] private AnimationClip JabAnimation;
        [FoldoutGroup("Animations"), SerializeField] private AnimationClip UseSupplyAnimation;
        [FoldoutGroup("Animations"), SerializeField] private AnimationClip TakeDamageAnimation;
        [FoldoutGroup("Animations"), SerializeField] private AnimationClip HealDamageAnimation;
        [FoldoutGroup("Animations"), SerializeField] private AnimationClip DieAnimation;
        [FoldoutGroup("Animations"), SerializeField] private AnimationClip ReviveAnimation;

        public float Slash() {
            Animazing.Play(SlashAnimation, 1);
            return SlashAnimation.length;
        }

        public float ChargeAttack() {
            Animazing.Play(ChargeAttackAnimation, 1);
            return ChargeAttackAnimation.length;
        }

        public float UseSupply() {
            Animazing.Play(UseSupplyAnimation, 1);
            return UseSupplyAnimation.length;
        }


        public float Revive() {
            Animazing.Play(ReviveAnimation, Single.PositiveInfinity);
            return ReviveAnimation.length;
        }

        public float Jab() {
            Animazing.Play(JabAnimation, 1);
            return JabAnimation.length;
        }

        public float TakeDamage() {
            if (TakeDamageAnimation == null) {
                Animazing.transform.DOPunchRotation(Vector3.one * 10, 0.25f);
                return 0.25f;
            }
            else {
                Animazing.Play(TakeDamageAnimation, 1);
                return TakeDamageAnimation.length;
            }
        }

        public float Heal() {
            Animazing.Play(HealDamageAnimation, 1);
            return HealDamageAnimation.length;
        }

        public float Die() {
            Animazing.Stop(1);
            Animazing.SetLayerDefaultAnimation(0, DieAnimation);
            Animazing.Play(DieAnimation, Single.PositiveInfinity);
            IsDead = true;
            
            /*
            // Disable intent game object
            var intentComponent = GetComponentInChildren<UIIntentDisplay>();
            intentComponent.gameObject.SetActive(false);
            */

            // Remove buffs
            List<string> buffsToRemove = new();

            foreach (var kvp in GetBuffs()) {
                buffsToRemove.Add(kvp.Key);
            }

            for (int i = 0; i < buffsToRemove.Count; i++) {
                if (!TryRemoveBuff(buffsToRemove[i], Int32.MaxValue)) {
                    Debug.LogError($"Failed to remove {buffsToRemove[i]} from {name}");
                }
            }

            /*
            UIStatusDisplay statusDisplay = GetComponentInChildren<UIStatusDisplay>();
            statusDisplay.gameObject.SetActive(false);
            */

            // Disable box collider
            if (TryGetComponent(out BoxCollider2D col)) {
                col.enabled = false;
            }

            return DieAnimation.length;
        }
    }
}