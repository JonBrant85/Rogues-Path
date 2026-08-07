using System;
using _Rogues_Path.World.Encounters;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Rogues_Path.World {
    public class WorldTile : MonoBehaviour {
        public EncounterData Encounter;
        public WorldTile NextTile;
        public SpriteRenderer IndicatorSprite;
        public Transform PawnContainer;
        public GameObject Model;

        public async UniTask PassedTile() {}

        public async UniTask StoppedOnTile() {
            await UIEncounterWindow.Instance.LoadEncounter(Encounter);
        }

        private void OnValidate() {
            if (Encounter != null) {
                
            }
        }
    }
}