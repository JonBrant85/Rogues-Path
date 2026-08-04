using _Rogues_Path.Pawns;
using Kryz.CharacterStats;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace _Rogues_Path.UI.CharacterScreen {
    public class UICharacterStat : MonoBehaviour {
        [SerializeField] private float adjustmentSpeed = 10;
        public Text LabelText;
        public Text ValueText;

        private CharacterStat stat;
        private float smoothedValue;

        public void SetCharacterStat(CharacterStat _stat, string _name) {
            LabelText.text = _name;
            stat = _stat;
            gameObject.SetActive(true);
        }

        public void UpdateValue() {
            var val = stat.Value;
            var diff = Mathf.Abs(val - smoothedValue);
            var dv = Mathf.Min(diff, Mathf.Max(diff * adjustmentSpeed * Time.deltaTime, 10f * adjustmentSpeed * Time.deltaTime));

            string colorString = (val - smoothedValue) switch {
                > 0 => "<color=green>",
                < 0 => "<color=red>",
                _ => "<color=white>"
            };
            smoothedValue += dv * Mathf.Sign(val - smoothedValue);
            ValueText.text = $"{colorString}{smoothedValue:N0}</color>";
        }
    }
}