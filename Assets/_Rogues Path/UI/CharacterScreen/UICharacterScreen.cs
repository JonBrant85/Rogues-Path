using System;
using System.Collections.Generic;
using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.Pawns;
using _Rogues_Path.UI.Slots;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using HeroEditor.Common.Enums;
using Kryz.CharacterStats;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace _Rogues_Path.UI.CharacterScreen {
    public class UICharacterScreen : Singleton<UICharacterScreen> {
        private Pawn player;
        public List<UIEquipmentSlot> Slots = new();
        public Transform StatsContainer;
        public UICharacterStat StatPrefab;
        public GameObject SpacerPrefab;

        [FoldoutGroup("References"), SerializeField] private Text CharacterNameText;
        [FoldoutGroup("References"), SerializeField] private Text CharacterClassText;


        private Dictionary<CharacterStats, UICharacterStat> stats = new();

        private void OnEnable() {
            EventBus.SubscribeTo<PawnStatChanged>(PawnStatChangedEventHandler);
        }

        private void OnDisable() {
            EventBus.UnsubscribeFrom<PawnStatChanged>(PawnStatChangedEventHandler);
        }

        private void PawnStatChangedEventHandler(ref PawnStatChanged eventData) {
            ClearCharacterStats();
            ShowCharacterStats();
        }

        public void SetPlayer(Pawn _player) {
            player = _player;

            // Set character name/class
            CharacterNameText.text = player.CharacterName;
            CharacterClassText.text = player.ClassName;

            // Setup equipment slots
            foreach (UIEquipmentSlot slot in Slots) {
                slot.Owner = _player;
            }


            ShowCharacterStats();
        }

        private void ShowCharacterStats() {
            BindCharacterStat(CharacterStats.MaxHealth, "Maximum Health");
            BindCharacterStat(CharacterStats.Strength, " Strength");
            BindCharacterStat(CharacterStats.Dexterity, "Dexterity");
            BindCharacterStat(CharacterStats.Intelligence, "Intelligence");
            BindCharacterStat(CharacterStats.Speed, "Speed");
        }

        private void BindCharacterStat(CharacterStats stat, string _name) {
            var uiStat = Instantiate(StatPrefab, StatsContainer);
            uiStat.SetCharacterStat(stat, player, _name);
            stats.Add(stat, uiStat);
        }

        private void ClearCharacterStats() {
            foreach (CharacterStats key in stats.Keys) {
                if (stats[key] != null) {
                    Destroy(stats[key].gameObject);
                }
            }
            stats.Clear();
        }
    }
}