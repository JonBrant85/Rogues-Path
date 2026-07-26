using System;
using System.Collections.Generic;
using _Rogues_Path.Pawns;
using DG.Tweening;
using DuloGames.UI;
using HeroEditor.Common.Data;
using HeroEditor.Common.Enums;
using Kryz.CharacterStats;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Rogues_Path.Equipment.Scripts {
    public enum CharacterStats {
        Intelligence,
        Strength,
        Dexterity,
        MaxHealth,
        Speed
    }

    [Serializable]
    public struct StatAndModifierPair {
        public CharacterStats Stat;
        public StatModifier Modifier;
    }

    public abstract class EquipmentBase : SerializedMonoBehaviour {
        public string Name;
        public string Description = "Default Description";
        public string FlavorText = "Something interesting and profound about the item here";
        public ItemSprite Sprite;
        public Color? SpriteColor;
        public Sprite Icon;
        public UIItemQuality Quality;
        public EquipmentPart EquipType = EquipmentPart.Armor;
        [FoldoutGroup("Debug")] public Pawn Owner;
        
        public List<StatAndModifierPair> Modifiers;


        private void OnEnable() {
            HandleSubscribing();
        }

        private void OnDisable() {
            HandleUnsubscribing();
        }

        protected void OnTriggerUI() {
            this.transform.parent.DOShakeRotation(0.5f, 10f);
        }

        abstract protected void HandleSubscribing();
        abstract protected void HandleUnsubscribing();
        
        public void ApplyModifiers(List<StatAndModifierPair> modifiers, Pawn owner) {
            foreach (StatAndModifierPair modifierPair in modifiers) {
                switch (modifierPair.Stat) {
                    case CharacterStats.Intelligence:
                        owner.Intelligence.AddModifier(modifierPair.Modifier);
                        break;
                    case CharacterStats.Strength:
                        owner.Strength.AddModifier(modifierPair.Modifier);
                        break;
                    case CharacterStats.Dexterity:
                        owner.Dexterity.AddModifier(modifierPair.Modifier);
                        break;
                    case CharacterStats.MaxHealth:
                        owner.MaxHealth.AddModifier(modifierPair.Modifier);
                        break;
                    case CharacterStats.Speed:
                        owner.Speed.AddModifier(modifierPair.Modifier);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void RemoveModifiers(List<StatAndModifierPair> modifiers, Pawn owner) {
            foreach (StatAndModifierPair modifierPair in modifiers) {
                switch (modifierPair.Stat) {
                    case CharacterStats.Intelligence:
                        owner.Intelligence.RemoveModifier(modifierPair.Modifier);
                        break;
                    case CharacterStats.Strength:
                        owner.Strength.RemoveModifier(modifierPair.Modifier);
                        break;
                    case CharacterStats.Dexterity:
                        owner.Dexterity.RemoveModifier(modifierPair.Modifier);
                        break;
                    case CharacterStats.MaxHealth:
                        owner.MaxHealth.RemoveModifier(modifierPair.Modifier);
                        break;
                    case CharacterStats.Speed:
                        owner.Speed.RemoveModifier(modifierPair.Modifier);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

}