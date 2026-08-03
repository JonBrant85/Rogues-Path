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
        [FoldoutGroup("References")] public UIStatusDisplay StatusDisplayPrefab;

        private UIStatusDisplay statusDisplay;

        private void Awake() {
            InitializeStats();
            InitializeAnimation();
            InitializeStatusDisplay();
        }

        private void InitializeStatusDisplay() {
            statusDisplay = Instantiate(StatusDisplayPrefab, transform);
            statusDisplay.SetOwner(this);
        }
    }
}