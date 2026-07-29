using System;
using _Rogues_Path.Buffs.Scripts;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using Guirao.UltimateTextDamage;
using Kryz.CharacterStats;
using UnityEngine;

namespace _Rogues_Path.Pawns {
    public partial class Pawn {
        public float CurrentHealth = 50;
        public bool IsDead = false;
        public IDStatDictionary Stats = new();
        public CharacterStat MaxHealth;
        public CharacterStat Strength;
        public CharacterStat Dexterity;
        public CharacterStat Intelligence;
        public CharacterStat Speed;

        public void InitializeStats() {
            CurrentHealth = MaxHealth.Value;
            Stats.TryAdd(MaxHealth.CharacterStatID, MaxHealth);
            Stats.TryAdd(Strength.CharacterStatID, Strength);
            Stats.TryAdd(Dexterity.CharacterStatID, Dexterity);
            Stats.TryAdd(Intelligence.CharacterStatID, Intelligence);
            Stats.TryAdd(Speed.CharacterStatID, Speed);
        }

        public float TakeDamage(int damage, Pawn instigator) {
            float duration = 0;
            PreMitigationDamageReceived preMitigationDamageReceivedEvent = new() {
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

                PostMitigationDamageReceived postMitigationDamageReceived = new() {
                    MitigatedDamage = mitigatedDamage,
                    Victims = new() {
                        this
                    },
                    Instigator = instigator
                };

                EventBus.RaiseImmediately(ref postMitigationDamageReceived);

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
            var healthChangedEvent = new HealthChanged {
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