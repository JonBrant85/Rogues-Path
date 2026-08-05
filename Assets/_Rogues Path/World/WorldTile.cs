using UnityEngine;

namespace _Rogues_Path.World {
    public class WorldTile : MonoBehaviour {
        public WorldTile NextTile;
        public SpriteRenderer IndicatorSprite;
        public Transform PawnContainer;
        public GameObject Model;
    }
}