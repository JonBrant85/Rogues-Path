using System;
using System.Collections.Generic;
using _Rogues_Path.Pawns;
using _Rogues_Path.UI.CharacterScreen;
using DG.Tweening;
using DuloGames.UI;
using HeroEditor.Common.Data;
using HeroEditor.Common.Enums;
using Kryz.CharacterStats;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Rogues_Path.Equipment.Scripts {
    [Serializable]
    public struct StatAndModifierPair {
        [FormerlySerializedAs("Stat")]
        public CharacterStatID StatID;
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
            for (int index = 0; index < modifiers.Count; index++) {
                StatAndModifierPair modifierPair = modifiers[index];

                if (owner.Stats.TryGetValue(modifierPair.StatID, out CharacterStat stat)) {
                    stat.AddModifier(modifierPair.Modifier);
                }
                else {
                    owner.Stats.Add(modifierPair.StatID, new CharacterStat {
                        CharacterStatID = modifierPair.StatID,
                        BaseValue = 0
                    });
                }
            }
        }

        public void RemoveModifiers(List<StatAndModifierPair> modifiers, Pawn owner) {
            // Remove modifiers in reverse juuuust in case
            for (int index = modifiers.Count - 1; index >= 0; index--) {
                StatAndModifierPair modifierPair = modifiers[index];

                if (owner.Stats.TryGetValue(modifierPair.StatID, out CharacterStat stat)) {
                    stat.RemoveModifier(modifierPair.Modifier);
                }
            }
        }
    }
}