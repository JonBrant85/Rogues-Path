using System;
using _Rogues_Path.Pawns;
using DuloGames.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Rogues_Path.CharacterSelection {
    public class CharacterSelectionSlot : MonoBehaviour {
        public PawnData PawnData;
        public int Index;

        private void OnMouseDown() {
            CharacterSelectionManager.Instance.SelectCharacter(this);
        }
    }
}