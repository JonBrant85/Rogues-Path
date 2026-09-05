using _Rogues_Path._Game;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using UnityEngine;

namespace _Rogues_Path.Pawns.Scripts {
    public static class PlayerHealthState {
        public static void Restore(Pawn player) {
            if (player == null)
                return;

            float maximumHealth = player.Stats[player.MaxHealthID].Value;
            float savedHealth = Game.Instance.PlayerCurrentHealth;

            if (savedHealth < 0f)
                savedHealth = maximumHealth;

            SetHealth(player, savedHealth);
        }

        public static void Save(Pawn player) {
            if (player == null)
                return;

            Game.Instance.PlayerCurrentHealth = player.CurrentHealth;
        }

        public static float Heal(Pawn player, float healthPercentage) {
            if (player == null || healthPercentage <= 0f)
                return 0f;

            float previousHealth = player.CurrentHealth;
            float maximumHealth = player.Stats[player.MaxHealthID].Value;
            float healthToRestore = maximumHealth * healthPercentage;

            SetHealth(player, previousHealth + healthToRestore);

            float restoredHealth = player.CurrentHealth - previousHealth;

            if (restoredHealth > 0f) {
                EventBus.Raise(new HealingApplied {
                    Victim = player,
                    Instigator = player,
                    Amount = restoredHealth
                });
            }

            return restoredHealth;
        }

        public static void ReconcileMaximumHealth(
            Pawn player,
            float previousMaximumHealth,
            bool updateSavedHealth) {

            if (player == null)
                return;

            float maximumHealth = player.Stats[player.MaxHealthID].Value;
            float maximumHealthDifference = maximumHealth - previousMaximumHealth;
            float adjustedHealth = maximumHealthDifference > 0f
                ? player.CurrentHealth + maximumHealthDifference
                : player.CurrentHealth;

            SetHealth(player, adjustedHealth, false);

            if (!updateSavedHealth)
                return;

            float savedHealth = Game.Instance.PlayerCurrentHealth;

            if (savedHealth < 0f)
                savedHealth = previousMaximumHealth;

            if (maximumHealthDifference > 0f)
                savedHealth += maximumHealthDifference;

            Game.Instance.PlayerCurrentHealth = Mathf.Clamp(savedHealth, 0f, maximumHealth);
        }

        private static void SetHealth(Pawn player, float health, bool updateSavedHealth = true) {
            float maximumHealth = player.Stats[player.MaxHealthID].Value;
            var healthChangedEvent = new HealthChanged {
                Victim = player,
                Instigator = player,
                OldHealth = player.CurrentHealth,
                NewHealth = Mathf.Clamp(health, 0f, maximumHealth)
            };

            EventBus.RaiseImmediately(ref healthChangedEvent);

            player.CurrentHealth = Mathf.Clamp(healthChangedEvent.NewHealth, 0f, maximumHealth);

            if (updateSavedHealth)
                Game.Instance.PlayerCurrentHealth = player.CurrentHealth;
        }
    }
}
