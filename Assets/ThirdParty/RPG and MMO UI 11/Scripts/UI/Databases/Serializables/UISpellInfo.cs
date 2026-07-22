using UnityEngine;
using System;
using _Rogues_Path.Commands;

namespace DuloGames.UI {
    [Serializable]
    public class UISpellInfo {
        [SerializeReference] public Command SpellCommand;
        public int ID;
        public string Name;
        public Sprite Icon;
        public string Description;
        public float Range;
        public float Cooldown;
        public float CastTime;
        public float PowerCost;

        [BitMask(typeof(UISpellInfo_Flags))]
        public UISpellInfo_Flags Flags;
    }
}