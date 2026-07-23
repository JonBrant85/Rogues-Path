using System;
using System.Collections.Generic;
using _Rogues_Path.Buffs.Scripts;
using _Rogues_Path.Pawns;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using UnityEngine;

namespace _Rogues_Path.UI {
    public class UIStatusDisplay : MonoBehaviour {
        public Pawn Owner;
        public Dictionary<string, PawnBuff> PawnBuffs = new();

        private void OnEnable() {
            EventBus.SubscribeTo<StatusChangedEvent>(StatusChangedEventHandler);
        }

        private void OnDisable() {
            EventBus.SubscribeTo<StatusChangedEvent>(StatusChangedEventHandler);
        }

        private void Update() {
            //transform.position = Camera.main!.WorldToScreenPoint(Owner.transform.position);
        }

        private void StatusChangedEventHandler(ref StatusChangedEvent eventData) {
            if (!eventData.Targets.Contains(Owner)) return;


            // This can fail if the buff isn't present in the didctionary, i.e. not present
            if (Owner.TryGetBuffCount(eventData.NewStatus, out int count)) {
                eventData.NewStatus.CountText.text = count.ToString();

                if (count == 0) {
                    eventData.NewStatus.OnBuffRemoved();
                    Destroy(PawnBuffs[eventData.NewStatus.Name]);
                    PawnBuffs.Remove(eventData.NewStatus.Name);
                }
            }
        }

        public void AddBuff(PawnBuff buffPrefab, int count) {
            if (PawnBuffs.ContainsKey(buffPrefab.Name)) {
                PawnBuffs[buffPrefab.Name].CountText.text = count.ToString();
            }
            else {
                var buff = Instantiate(buffPrefab, transform);
                buff.Owner = Owner;
                PawnBuffs.Add(buff.Name, buff);
                buff.CountText.text = count.ToString();
                buff.Image.sprite = buff.Sprite;
                buff.OnBuffAdded(Owner, count);
            }
        }

        public void RemoveBuff(PawnBuff buffPrefab) {
            if (Owner.TryGetBuffCount(buffPrefab, out int buffCount)) {
                PawnBuffs[buffPrefab.Name].CountText.text = buffCount.ToString();
            }
            else {
                Destroy(PawnBuffs[buffPrefab.Name].gameObject);
                PawnBuffs.Remove(buffPrefab.Name);
            }
        }
    }
}