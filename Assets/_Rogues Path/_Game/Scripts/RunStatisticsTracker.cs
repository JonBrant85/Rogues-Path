using _Rogues_Path.Pawns.Scripts;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using UnityEngine;

namespace _Rogues_Path._Game {
    public sealed class RunStatisticsTracker {
        private readonly Game game;
        private Pawn player;
        private Pawn enemy;
        private bool isTracking;
        private bool isSubscribed;

        public RunStatisticsTracker(Game game) {
            this.game = game;
        }

        public void Subscribe() {
            if (isSubscribed)
                return;

            EventBus.SubscribeTo<RunStarted>(RunStartedHandler);
            EventBus.SubscribeTo<RunEnded>(RunEndedHandler);
            EventBus.SubscribeTo<RunPawnsChanged>(RunPawnsChangedHandler);
            EventBus.SubscribeTo<TileTraversed>(TileTraversedHandler);
            EventBus.SubscribeTo<CombatCleared>(CombatClearedHandler);
            EventBus.SubscribeTo<TreasureClaimed>(TreasureClaimedHandler);
            EventBus.SubscribeTo<OrbConsumed>(OrbConsumedHandler);
            EventBus.SubscribeTo<DamageApplied>(DamageAppliedHandler);
            EventBus.SubscribeTo<HealingApplied>(HealingAppliedHandler);
            isSubscribed = true;
        }

        public void Unsubscribe() {
            if (!isSubscribed)
                return;

            EventBus.UnsubscribeFrom<RunStarted>(RunStartedHandler);
            EventBus.UnsubscribeFrom<RunEnded>(RunEndedHandler);
            EventBus.UnsubscribeFrom<RunPawnsChanged>(RunPawnsChangedHandler);
            EventBus.UnsubscribeFrom<TileTraversed>(TileTraversedHandler);
            EventBus.UnsubscribeFrom<CombatCleared>(CombatClearedHandler);
            EventBus.UnsubscribeFrom<TreasureClaimed>(TreasureClaimedHandler);
            EventBus.UnsubscribeFrom<OrbConsumed>(OrbConsumedHandler);
            EventBus.UnsubscribeFrom<DamageApplied>(DamageAppliedHandler);
            EventBus.UnsubscribeFrom<HealingApplied>(HealingAppliedHandler);
            isSubscribed = false;
        }

        private void RunStartedHandler(ref RunStarted eventData) {
            game.ResetRunStatistics();
            ClearPawns();
            isTracking = true;
        }

        private void RunEndedHandler(ref RunEnded eventData) {
            isTracking = false;
            ClearPawns();
        }

        private void RunPawnsChangedHandler(ref RunPawnsChanged eventData) {
            if (!isTracking)
                return;

            // These references come from the scene managers after spawning their live pawns.
            player = eventData.Player;
            enemy = eventData.Enemy;
        }

        private void TileTraversedHandler(ref TileTraversed eventData) {
            if (isTracking && player != null && enemy == null && eventData.Player == player)
                game.TilesTraveled++;
        }

        private void CombatClearedHandler(ref CombatCleared eventData) {
            if (!isTracking || player == null || enemy == null || player.IsDead || !enemy.IsDead)
                return;

            game.CombatsCleared++;
            // Clear the encounter immediately, so duplicate completions and late projectiles do not count.
            ClearPawns();
        }

        private void TreasureClaimedHandler(ref TreasureClaimed eventData) {
            if (isTracking && eventData.Equipment != null)
                game.TreasuresClaimed++;
        }

        private void OrbConsumedHandler(ref OrbConsumed eventData) {
            if (isTracking && eventData.Amount > 0)
                game.OrbsUsed += eventData.Amount;
        }

        private void DamageAppliedHandler(ref DamageApplied eventData) {
            if (!isTracking || player == null || eventData.Amount <= 0f)
                return;

            if (eventData.Victim == player) {
                game.DamageTaken += eventData.Amount;
            }
            else if (enemy != null && eventData.Victim == enemy && eventData.Instigator == player) {
                game.DamageDealt += eventData.Amount;
                game.BiggestSingleHit = Mathf.Max(game.BiggestSingleHit, eventData.Amount);
            }
        }

        private void HealingAppliedHandler(ref HealingApplied eventData) {
            if (isTracking && player != null && eventData.Victim == player && eventData.Amount > 0f)
                game.HealthRestored += eventData.Amount;
        }

        private void ClearPawns() {
            player = null;
            enemy = null;
        }
    }
}
