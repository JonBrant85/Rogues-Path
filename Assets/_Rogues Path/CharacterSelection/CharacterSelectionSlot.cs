using System;
using _Rogues_Path.Pawns;
using DuloGames.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Rogues_Path.CharacterSelection {
    public class CharacterSelectionSlot : MonoBehaviour {
        public Pawn Pawn;
        public int Index;

        private void Awake() {
            Pawn = GetComponentInChildren<Pawn>();
        }

        private void OnMouseDown() {
            CharacterSelectionManager.Instance.SelectCharacter(this);
        }
    }
}