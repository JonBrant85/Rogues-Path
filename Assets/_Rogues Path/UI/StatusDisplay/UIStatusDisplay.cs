using System;
using System.Collections.Generic;
using _Rogues_Path.Buffs.Scripts;
using _Rogues_Path.Pawns;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using UnityEngine;

namespace _Rogues_Path.UI {
    public class UIStatusDisplay : MonoBehaviour {
        public Dictionary<string, PawnBuff> PawnBuffs = new();

        private Pawn owner;

        private void OnEnable() {


            EventBus.SubscribeTo<StatusChanged>(StatusChangedEventHandler);

            if (TryGetComponent(out Canvas canvas)) {
                canvas.worldCamera = Camera.main;
            }
        }

        private void OnDisable() {
            EventBus.SubscribeTo<StatusChanged>(StatusChangedEventHandler);
        }

        private void StatusChangedEventHandler(ref StatusChanged eventData) {
            if (!eventData.Targets.Contains(owner)) return;

            // This can fail if the buff isn't present in the dictionary, i.e. not present
            if (owner.TryGetBuffCount(eventData.NewStatus, out int count)) {
                eventData.NewStatus.CountText.text = count.ToString();

                if (count == 0) {
                    eventData.NewStatus.OnBuffRemoved();
                    Destroy(PawnBuffs[eventData.NewStatus.Name]);
                    PawnBuffs.Remove(eventData.NewStatus.Name);
                }
            }
        }

        public void SetOwner(Pawn _owner) {
            owner = _owner;
        }

        public void AddBuff(PawnBuff buffPrefab, int count) {
            if (PawnBuffs.ContainsKey(buffPrefab.Name)) {
                PawnBuffs[buffPrefab.Name].CountText.text = count.ToString();
            }
            else {
                var buff = Instantiate(buffPrefab, transform);
                buff.Owner = owner;
                PawnBuffs.Add(buff.Name, buff);
                buff.CountText.text = count.ToString();
                buff.Image.sprite = buff.Sprite;
                buff.OnBuffAdded(owner, count);
            }
        }

        public void RemoveBuff(PawnBuff buffPrefab) {
            if (owner.TryGetBuffCount(buffPrefab, out int buffCount)) {
                PawnBuffs[buffPrefab.Name].CountText.text = buffCount.ToString();
            }
            else {
                Destroy(PawnBuffs[buffPrefab.Name].gameObject);
                PawnBuffs.Remove(buffPrefab.Name);
            }
        }
    }
}