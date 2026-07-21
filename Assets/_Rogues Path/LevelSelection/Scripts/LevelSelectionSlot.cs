using _Rogues_Path.CharacterSelection;
using _Rogues_Path.Pawns;
using UnityEngine;

namespace _Rogues_Path.LevelSelection {
    public class LevelSelectionSlot : MonoBehaviour {
        public LevelData LevelData;
        public int Index;


        private void OnMouseDown() {
            LevelSelectionManager.Instance.SelectLevel(this);
        }
    }
}