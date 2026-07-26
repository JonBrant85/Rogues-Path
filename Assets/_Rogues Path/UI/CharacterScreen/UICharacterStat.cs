using System;
using System.Diagnostics;
using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.Pawns;
using Kryz.CharacterStats;
using UnityEngine;
using UnityEngine.UI;

namespace _Rogues_Path.UI.CharacterScreen {
    public class UICharacterStat: MonoBehaviour {
        public Text LabelText;
        public Text ValueText;
        public Pawn Owner;
        public void SetCharacterStat(CharacterStats stat, Pawn owner, string _name) {
            LabelText.text = _name;
            Owner = owner;
            
            ValueText.text = stat switch {
                CharacterStats.Intelligence => owner.Intelligence.Value.ToString(),
                CharacterStats.Strength => owner.Strength.Value.ToString(),
                CharacterStats.Dexterity => owner.Dexterity.Value.ToString(),
                CharacterStats.MaxHealth => owner.MaxHealth.Value.ToString(),
                CharacterStats.Speed => owner.Speed.Value.ToString(),
                _ => throw new ArgumentOutOfRangeException(nameof(stat), stat, null)
            };
            
            
            gameObject.SetActive(true);
            
        }
    }
}