using _Rogues_Path.World.Encounters;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Rogues_Path.World {
    public class WorldTile : MonoBehaviour {
        public EncounterData Encounter;
        public WorldTile NextTile;
        public SpriteRenderer IndicatorSprite;
        public Transform PawnContainer;
        public GameObject Model;

        [FormerlySerializedAs("EnemyContainer")]
        [SerializeField] private Transform EncounterContainer;

        private void Start() {
            if (EncounterContainer == null) {
                Debug.LogError($"{name}: EncounterContainer is not assigned.");
                return;
            }

            if (Encounter == null) {
                Debug.LogError($"{name}: Encounter is not assigned.");
                return;
            }

            Encounter = Instantiate(Encounter);

            if (Encounter.WorldIndicatorSprite != null && IndicatorSprite != null)
                IndicatorSprite.sprite = Encounter.WorldIndicatorSprite;

            Encounter.Initialize(EncounterContainer);
        }

        public async UniTask PassedTile() {}

        public async UniTask StoppedOnTile() {
            if (Encounter == null) {
                Debug.LogError($"{name}: Cannot load an unassigned encounter.");
                return;
            }

            await UIEncounterWindow.Instance.LoadEncounter(Encounter);
        }

    }
}
