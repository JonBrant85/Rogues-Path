using System;
using System.Linq;
using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.UI.Slots;
using UnityEngine;

namespace _Rogues_Path.UI.CharacterScreen {
    public class TestAssignUIEquipmentSlot : MonoBehaviour {
        public UIEquipmentSlot slot;
        public EquipmentBase Equipment;

        private void Start() {
            slot.Assign(Equipment);
            Debug.Log($"Slot filled with : {slot.GetEquipmentInfo().Name}");
        }
    }
}