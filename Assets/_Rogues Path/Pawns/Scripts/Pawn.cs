using _Rogues_Path.Pawns.Scripts.Brains;
using _Rogues_Path.UI;
using Assets.HeroEditor4D.Common.Scripts.CharacterScripts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Rogues_Path.Pawns.Scripts {
    public partial class Pawn : MonoBehaviour {
        public string CharacterName;
        public string ClassName;
        [FoldoutGroup("References")] public Character4D Character;
        [FoldoutGroup("References")] public PawnBrain Brain;
        /// Status Display. Handles buffs and Health displays. May be null for pawn previews
        [FoldoutGroup("References")] public UIStatusDisplay StatusDisplay;

        private void Awake() {
            InitializeStats();
            InitializeAnimation();
            InitializeStatusDisplay();
        }

        private void OnMouseEnter() {
            if (StatusDisplay != null) StatusDisplay.HealthDisplay.Show();
        }

        private void OnMouseExit() {
            if (StatusDisplay != null) StatusDisplay.HealthDisplay.Hide();
        }

        private void InitializeStatusDisplay() {
            if (StatusDisplay != null) StatusDisplay.SetOwner(this);
        }
    }
}