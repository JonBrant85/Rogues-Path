using System;
using _Rogues_Path.Pawns;
using _Rogues_Path.Pawns.Scripts;
using Assets.HeroEditor.Common.Scripts.CharacterScripts;
using Assets.HeroEditor4D.Common.Scripts.CharacterScripts;
using DuloGames.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Rogues_Path.CharacterSelection {
    public class CharacterSelectionSlot : MonoBehaviour {
        public Character4D Character;
        public PawnData PawnData;
        public int Index;

        private void Awake() {
            foreach (var equipment in PawnData.StartingEquipment) {
                Character.Equip(equipment.ItemSprite, equipment.EquipType);
            }

            if (Character.TryGetComponent(out Pawn pawn)) {
                pawn.StatusDisplay = null;
            }
        }

        private void OnMouseDown() {
            CharacterSelectionManager.Instance.SelectCharacter(this);
        }
    }
}