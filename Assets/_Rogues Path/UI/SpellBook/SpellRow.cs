using DuloGames.UI;
using UnityEngine;
using UnityEngine.UI;

namespace _Rogues_Path.UI {
    public class SpellRow : MonoBehaviour {
        [SerializeField] private UISpellSlot m_Slot;
        [SerializeField] private Text m_NameText;
        [SerializeField] private Text m_RankText;
        [SerializeField] private Text m_DescriptionText;

        public void AssignSpell(UISpellInfo spell) {
            if (this.m_Slot != null) this.m_Slot.Assign(spell);
            if (this.m_NameText != null) this.m_NameText.text = spell.Name;
            if (this.m_RankText != null) this.m_RankText.text = Random.Range(1, 6).ToString();
            if (this.m_DescriptionText != null) this.m_DescriptionText.text = spell.Description;
        }
    }
}