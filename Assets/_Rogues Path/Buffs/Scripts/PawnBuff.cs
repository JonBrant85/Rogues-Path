using _Rogues_Path.Pawns;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Rogues_Path.Buffs.Scripts {
    public class PawnBuff : SerializedMonoBehaviour, IPointerEnterHandler {
        [HideInInspector] public Pawn Owner;
        public string Name;
        public Sprite Sprite;
        public Image Image;
        public TextMeshProUGUI CountText;

        public virtual void OnBuffAdded(Pawn owner, int count) {
            Owner = owner;
            CountText.text = count.ToString();
        }

        public virtual void OnBuffRemoved() {
            if (Owner.TryGetBuffCount(this, out int buffCount)) {
                CountText.text = buffCount.ToString();
            }
            else {
                Debug.Log($"Failed to get buff count!");
            }
        }

        public void OnPointerEnter(PointerEventData eventData) {
            Debug.Log($"Pointer enter!");
        }
    }
}