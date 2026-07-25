using Kryz.CharacterStats;
using UnityEngine;
using UnityEngine.UI;

namespace _Rogues_Path.UI.CharacterScreen {
    public class UICharacterStat: MonoBehaviour {
        public Text LabelText;
        public Text ValueText;
        public void SetCharacterStat(CharacterStat stat, string _name) {
            LabelText.text = _name;
            ValueText.text = stat.Value.ToString();
            gameObject.SetActive(true);
        }
    }
}