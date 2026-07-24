using System;
using System.Linq;
using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.UI.Slots;
using UnityEngine;

namespace _Rogues_Path.UI.CharacterScreen {
    public class TestAssignUIEquipmentSlot : MonoBehaviour {
        public UIEquipmentSlot slot;

        private void Start() {
            var db = EquipmentDatabase.Instance;
            var equipment = db.Equipment.FirstOrDefault(equipEntry => equipEntry.EquipType == slot.EquipType);
            slot.Assign(equipment);
        }
    }
}