using _Rogues_Path.Pawns;
using DG.Tweening;
using DuloGames.UI;
using HeroEditor.Common.Data;
using HeroEditor.Common.Enums;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Rogues_Path.Equipment.Scripts {
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
    }
}