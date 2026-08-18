using System;
using System.Collections.Generic;
using _Rogues_Path.Buffs.Scripts;
using _Rogues_Path.Pawns;
using _Rogues_Path.Pawns.Scripts;
using _Rogues_Path.UI.CharacterScreen;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Rogues_Path.UI {
    /// <summary>
    /// Displays Health and Buffs
    /// </summary>
    public class UIStatusDisplay : MonoBehaviour, IPointerEnterHandler {
        public Dictionary<string, PawnBuff> PawnBuffs = new();
        public UIHealthDisplay HealthDisplay;

        private Pawn owner;
        [SerializeField] private CharacterStatID MaximumHealth;

        private void OnEnable() {
            EventBus.SubscribeTo<StatusChanged>(StatusChangedEventHandler);
            EventBus.SubscribeTo<HealthChanged>(HealthChangedEventHandler);

            // Update camera so mouse over events work
            if (TryGetComponent(out Canvas canvas)) {
                canvas.worldCamera = Camera.main;
            }
        }

        private void OnDisable() {
            EventBus.UnsubscribeFrom<StatusChanged>(StatusChangedEventHandler);
            EventBus.UnsubscribeFrom<HealthChanged>(HealthChangedEventHandler);
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

        private void HealthChangedEventHandler(ref HealthChanged eventData) {
            if (eventData.Victim != owner) return;

            HealthDisplay.TweenFillAmount(eventData.NewHealth/eventData.Victim.Stats[MaximumHealth].Value);
        }

        public void SetOwner(Pawn _owner) {
            owner = _owner;
            
            // Update Unit Name
            HealthDisplay.UnitNameText.text = owner.CharacterName;
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

        public void OnPointerEnter(PointerEventData eventData) {
            Debug.Log($"Mouse over {owner.CharacterName}");
        }
    }
}