using System;
using System.Collections.Generic;
using _Rogues_Path.Pawns;
using _Rogues_Path.UI.Slots;
using _Rogues_Path.Utilities;
using HeroEditor.Common.Enums;
using Kryz.CharacterStats;
using UnityEngine;

namespace _Rogues_Path.UI.CharacterScreen {
    public class UICharacterScreen : Singleton<UICharacterScreen> {
        private Pawn player;
        public List<UIEquipmentSlot> Slots = new();
        public Transform StatsContainer;
        public UICharacterStat StatPrefab;
        public GameObject SpacerPrefab;

        public void SetPlayer(Pawn _player) {
            player = _player;
            
            // Init character stats
            BindCharacterStat(player.MaxHealth, "Maximum Health");
            Instantiate(SpacerPrefab, StatsContainer);
            BindCharacterStat(player.Strength, "Strength");
            BindCharacterStat(player.Dexterity, "Dexterity");
            BindCharacterStat(player.Intelligence, "Intelligence");
            BindCharacterStat(player.Speed, "Speed");
        }

        private void BindCharacterStat(CharacterStat stat, string _name) {
            var uiStat = Instantiate(StatPrefab, StatsContainer);
            uiStat.SetCharacterStat(stat, _name);
        }
    }
}