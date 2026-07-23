using _Rogues_Path.Pawns;
using _Rogues_Path.Pawns.Brains;
using _Rogues_Path.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Rogues_Path.UI {
    public class UISpellBook : Singleton<UISpellBook> {
        [FoldoutGroup("References"), SerializeField] private SpellRow rowPrefab;
        [FoldoutGroup("References"), SerializeField] private Transform rowContainer;
        private Pawn player;

        public void SetPlayer(Pawn _player) {
            player = _player;

            if (player.TryGetComponent(out PlayerBrain brain)) {
                foreach (var spellInfo in brain.Spells) {
                    var currentRow = Instantiate(rowPrefab, rowContainer);
                    currentRow.AssignSpell(spellInfo);
                    currentRow.gameObject.SetActive(true);
                }
            }
        }
    }
}