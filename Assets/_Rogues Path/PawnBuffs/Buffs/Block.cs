using System;
using _Rogues_Path.Buffs.Scripts;
using _Rogues_Path.Pawns;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using Guirao.UltimateTextDamage;
using UnityEngine;

namespace _Rogues_Path.PawnBuffs.Buffs {
    public class Block : PawnBuff {
        public override void OnBuffAdded(Pawn owner, int count) {
            base.OnBuffAdded(owner, count);
            EventBus.SubscribeTo<PreMitigationDamageReceivedEvent>(EventHandler);
        }

        public override void OnBuffRemoved() {
            EventBus.UnsubscribeFrom<PreMitigationDamageReceivedEvent>(EventHandler);
        }

        private void EventHandler(ref PreMitigationDamageReceivedEvent eventData) {
            eventData.UnmitigatedDamage = 0;
            UltimateTextDamageManager.Instance.Add("Blocked", transform.position + Owner.DamageOffset, "Damage");

            Owner.TryRemoveBuff(this, 1);
        }
    }
}