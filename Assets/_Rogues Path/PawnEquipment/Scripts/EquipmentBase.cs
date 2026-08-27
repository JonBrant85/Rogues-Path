using System;
using System.Collections.Generic;
using _Rogues_Path.Pawns;
using _Rogues_Path.Pawns.Scripts;
using _Rogues_Path.UI.CharacterScreen;
using DG.Tweening;
using DuloGames.UI;
using Kryz.CharacterStats;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using EquipmentPart = Assets.HeroEditor4D.Common.Scripts.Enums.EquipmentPart;
using ItemSprite = Assets.HeroEditor4D.Common.Scripts.Data.ItemSprite;

namespace _Rogues_Path.Equipment.Scripts {
    [Serializable]
    public struct StatAndModifierPair {
        [FormerlySerializedAs("Stat")]
        public CharacterStatID StatID;
        public StatModifier Modifier;
    }

    [Serializable]
    public class EquipmentBase : SerializedMonoBehaviour {
        public string Name;
        public string Description = "Default Description";
        public string FlavorText = "Something interesting and profound about the item here";
        [FormerlySerializedAs("Sprite")]
        public ItemSprite ItemSprite;
        public Color? SpriteColor;
        public Sprite Icon;
        public UIItemQuality Quality;
        public EquipmentPart EquipType = EquipmentPart.Armor;

        [FoldoutGroup("Debug")]
        public Pawn Owner;

        public List<StatAndModifierPair> Modifiers;


        private void OnEnable() {
            HandleSubscribing();
        }

        private void OnDisable() {
            HandleUnsubscribing();
        }

        protected void OnTriggerUI() {
            if (transform.parent != null) {
                transform.parent.DOShakeRotation(0.5f, 10f);
            }
        }

        protected virtual void HandleSubscribing() {}

        protected virtual void HandleUnsubscribing() {}


        public void ApplyModifiers(List<StatAndModifierPair> modifiers, Pawn owner) {
            if (modifiers == null || owner == null)
                return;

            foreach (StatAndModifierPair modifierPair in modifiers) {
                if (!owner.Stats.TryGetValue(modifierPair.StatID, out CharacterStat stat)) {

                    stat = new CharacterStat {
                        CharacterStatID = modifierPair.StatID,
                        BaseValue = 0
                    };

                    owner.Stats.Add(modifierPair.StatID, stat);
                }

                // Important:
                // The old implementation didn't add the modifier when
                // the CharacterStat itself had to be created.
                stat.AddModifier(modifierPair.Modifier);
            }
        }

        public void RemoveModifiers(List<StatAndModifierPair> modifiers, Pawn owner) {
            if (modifiers == null || owner == null)
                return;

            for (int i = modifiers.Count - 1; i >= 0; i--) {
                StatAndModifierPair modifierPair = modifiers[i];

                if (owner.Stats.TryGetValue(modifierPair.StatID, out CharacterStat stat)) {

                    stat.RemoveModifier(modifierPair.Modifier);
                }
            }
        }
    }
}