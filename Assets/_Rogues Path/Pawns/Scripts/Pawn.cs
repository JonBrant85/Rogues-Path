using System;
using _Rogues_Path.Pawns.Brains;
using _Rogues_Path.UI;
using Assets.HeroEditor.Common.Scripts.CharacterScripts;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Rogues_Path.Pawns {
    public partial class Pawn : MonoBehaviour {
        public string CharacterName;
        public string ClassName;
        [FoldoutGroup("References")] public Character Character;
        [FoldoutGroup("References")] public PawnBrain Brain;
        /// Status Display. Handles buffs and Health displays. May be null for pawn previews
        [FoldoutGroup("References")] public UIStatusDisplay StatusDisplay;

        private void Awake() {
            InitializeStats();
            InitializeAnimation();
            InitializeStatusDisplay();
        }

        private void OnMouseEnter() {
        }

        private void OnMouseExit() {
        }

        private void InitializeStatusDisplay() {
            if (StatusDisplay != null) StatusDisplay.SetOwner(this);
        }
    }
}