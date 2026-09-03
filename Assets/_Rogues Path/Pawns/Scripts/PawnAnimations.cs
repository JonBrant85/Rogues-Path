using System;
using System.Collections.Generic;
using _Rogues_Path.UI;
using DG.Tweening;
using OldOdin;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Rogues_Path.Pawns.Scripts {
    public partial class Pawn {
        [FoldoutGroup("Damage Text Offset")] public Vector3 DamageOffset = new Vector3(-1, 0, 0);
        [FoldoutGroup("Damage Text Offset")] public Vector3 HealingOffset = new Vector3(1, 0, 0);

        [FoldoutGroup("Animations"), SerializeField] private AnimationClip IdleAnimation;
        [FoldoutGroup("Animations"), SerializeField] private AnimationClip TakeDamageAnimation;
        [FoldoutGroup("Animations"), SerializeField] private AnimationClip HealDamageAnimation;
        [FoldoutGroup("Animations"), SerializeField] private AnimationClip DieAnimation;
        [FoldoutGroup("References"), SerializeField] private Animazing animazing;

        private void InitializeAnimation() {
            animazing.SetLayerDefaultAnimation(0, IdleAnimation);
        }

        public float PlayAnimation(AnimationClip clip) {
            animazing.PlayLayer(1, clip, int.MaxValue);
            return clip.length;
        }

        public float TakeDamage() {
            if (TakeDamageAnimation == null) {
                animazing.transform.DOPunchRotation(Vector3.one * 10, 0.25f);
                return 0.25f;
            }
            else {
                animazing.Play(TakeDamageAnimation, 1);
                return TakeDamageAnimation.length;
            }
        }

        public float Heal() {
            animazing.Play(HealDamageAnimation, 1);
            return HealDamageAnimation.length;
        }

        public float Die() {
            /*
            animazing.Stop(1);
            animazing.SetLayerDefaultAnimation(0, DieAnimation);
            animazing.Play(DieAnimation, Single.PositiveInfinity);
            */
            Character.AnimationManager.Die();
            IsDead = true;

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

            if (StatusDisplay != null) StatusDisplay.gameObject.SetActive(false);

            // Disable box collider
            if (TryGetComponent(out BoxCollider2D col)) {
                col.enabled = false;
            }

            return DieAnimation.length;
        }
    }
}
