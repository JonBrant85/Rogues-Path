using System;
using _Rogues_Path.Pawns;
using Assets.HeroEditor.Common.Scripts.CharacterScripts;
using DuloGames.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Rogues_Path.CharacterSelection {
    public class CharacterSelectionSlot : MonoBehaviour {
        public Character Character;
        public PawnData PawnData;
        public int Index;

        private void Awake() {
            foreach (var equipment in PawnData.StartingEquipment) {
                Character.Equip(equipment.Sprite, equipment.EquipType);
            }
        }

        private void OnMouseDown() {
            CharacterSelectionManager.Instance.SelectCharacter(this);
        }
    }
}