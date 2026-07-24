using _Rogues_Path.Pawns.Brains;
using Assets.HeroEditor.Common.Scripts.CharacterScripts;
using UnityEngine;

namespace _Rogues_Path.Pawns {
    public partial class Pawn : MonoBehaviour {
        public string CharacterName;
        public string ClassName;
        [SerializeField]
        public Character character;
        public PawnBrain Brain;

        private void Awake() {
           InitializeStats();
           Animazing.SetLayerDefaultAnimation(0, IdleAnimation);
        }
    }
}