using System;
using System.Collections.Generic;
using _Rogues_Path.UI.Slots;
using _Rogues_Path.Utilities;
using HeroEditor.Common.Enums;

namespace _Rogues_Path.UI.CharacterScreen {
    public class UICharacterScreen : Singleton<UICharacterScreen> {
        public List<UIEquipmentSlot> Slots = new();

        private void Start() {
            throw new NotImplementedException();
        }
    }
}