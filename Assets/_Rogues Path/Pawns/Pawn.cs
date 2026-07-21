using Assets.HeroEditor.Common.Scripts.CharacterScripts;
using UnityEngine;

namespace _Rogues_Path.Pawns {
    public partial class Pawn : MonoBehaviour {
        [SerializeField]
        public Character character;

        private void Awake() {
           // InitializeStats();
            //Animazing.SetLayerDefaultAnimation(0, IdleAnimation);
        }
    }
}