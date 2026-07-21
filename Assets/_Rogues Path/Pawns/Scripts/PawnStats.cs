using System;
using _Rogues_Path.Buffs.Scripts;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using Guirao.UltimateTextDamage;
using Kryz.CharacterStats;
using UnityEngine;

namespace _Rogues_Path.Pawns {
    public partial class Pawn {
        public bool IsDead = false;
        public float CurrentHealth = 50;
        public CharacterStat MaxHealth;
        public int CurrentShields;

        public void InitializeStats() {
            CurrentHealth = MaxHealth.Value;
        }

        public float TakeDamage(int damage, Pawn instigator) {
            float duration = 0;
            PreMitigationDamageReceivedEvent preMitigationDamageReceivedEvent = new() {
                UnmitigatedDamage = damage,
                Victim = this,
                Instigator = instigator
            };

            EventBus.RaiseImmediately(ref preMitigationDamageReceivedEvent);

            duration = preMitigationDamageReceivedEvent.UnmitigatedDamage switch {
                0 => 0,
                > 0 => HandleDamage(),
                < 0 => HandleHealing()
            };

            float HandleDamage() {
                int mitigatedDamage = preMitigationDamageReceivedEvent.UnmitigatedDamage;

                // Mitigate damage via block. ToDo: Have buff listen for this and mutate the damage instead?
                if (BuffsDatabase.Instance.TryGetBuffByName("Block", out PawnBuff BlockBuffReference) && TryGetBuffCount(BlockBuffReference, out int count)) {
                    mitigatedDamage = (int)Mathf.Clamp(mitigatedDamage - count, 0, float.PositiveInfinity);
                }

                TryRemoveBuff(BlockBuffReference, preMitigationDamageReceivedEvent.UnmitigatedDamage);

                PostMitigationDamageReceivedEvent postMitigationDamageReceivedEvent = new() {
                    MitigatedDamage = mitigatedDamage,
                    Victims = new() {
                        this
                    },
                    Instigator = instigator
                };

                EventBus.RaiseImmediately(ref postMitigationDamageReceivedEvent);

                if (mitigatedDamage > 0) {
                    ReceiveDamage(mitigatedDamage, preMitigationDamageReceivedEvent.Instigator);
                    UltimateTextDamageManager.Instance.Add(mitigatedDamage.ToString(), transform.position + DamageOffset, "Damage");
                    return TakeDamage();
                }
                else {
                    return 0;
                }
            }

            float HandleHealing() {
                ReceiveDamage(preMitigationDamageReceivedEvent.UnmitigatedDamage, preMitigationDamageReceivedEvent.Instigator);
                UltimateTextDamageManager.Instance.Add(
                    $"<color=green>{Mathf.Abs(preMitigationDamageReceivedEvent.UnmitigatedDamage).ToString()}</color>",
                    transform.position + HealingOffset,
                    "Healing");
                return Heal();
            }


            return duration;
        }


        public void ReceiveDamage(int damage, Pawn instigator) {
            var healthChangedEvent = new HealthChangedEvent {
                Victim = this,
                Instigator = instigator,
                NewHealth = CurrentHealth - damage,
                OldHealth = CurrentHealth
            };
            EventBus.RaiseImmediately(ref healthChangedEvent);

            CurrentHealth = Mathf.Clamp(healthChangedEvent.NewHealth, 0, MaxHealth.Value);

            if (CurrentHealth <= 0) {
                Die();
            }
        }
    }
}