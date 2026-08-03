using UnityEngine;
using System;
using _Rogues_Path._Game;
using _Rogues_Path.Commands;
using Sirenix.OdinInspector;

namespace DuloGames.UI {
    [CreateAssetMenu(fileName = "New UISpellInfo", menuName = Game.Name + "/Data/" + nameof(UISpellInfo))]
    public class UISpellInfo : ScriptableObject {
        [SerializeReference] public Command SpellCommand;

        public string Name;
        public Sprite Icon;
        public string Description;
        public float Cooldown;
        
        [BitMask(typeof(UISpellInfo_Flags))]
        [FoldoutGroup("Unused")] public UISpellInfo_Flags Flags;
        [FoldoutGroup("Unused")] public int ID;
        [FoldoutGroup("Unused")] public float Range;
        [FoldoutGroup("Unused")] public float CastTime;
        [FoldoutGroup("Unused")] public float PowerCost;
    }
}