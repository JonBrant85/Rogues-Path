using _Rogues_Path.World.Encounters;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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

        private EncounterData runtimeEncounter;
        private Transform runtimeEncounterVisual;
        private bool hasStarted;

        public bool CanInitializeEncounter => EncounterContainer != null;

        private void Start() {
            hasStarted = true;

            if (!TryInitializeEncounter())
                Debug.LogError($"{name}: Failed to initialize encounter.");
        }

        public bool TrySetEncounter(EncounterData encounter) {
            if (encounter == null) {
                Debug.LogError($"{name}: Cannot assign a null encounter.");
                return false;
            }

            if (hasStarted && EncounterContainer == null) {
                Debug.LogError($"{name}: EncounterContainer is not assigned.");
                return false;
            }

            Encounter = encounter;

            return !hasStarted || TryInitializeEncounter();
        }

        public void ClearRuntimeEncounter() {
            if (runtimeEncounterVisual != null) {
                runtimeEncounterVisual.DOKill();
                runtimeEncounterVisual = null;
            }

            if (runtimeEncounter != null) {
                Destroy(runtimeEncounter);
                runtimeEncounter = null;
            }

            for (int i = EncounterContainer.childCount - 1; i >= 0; i--) {
                GameObject previousVisual = EncounterContainer.GetChild(i).gameObject;
                previousVisual.SetActive(false);
                previousVisual.transform.SetParent(null);
                Destroy(previousVisual);
            }

            if (IndicatorSprite != null)
                IndicatorSprite.sprite = null;
        }

        private bool TryInitializeEncounter() {
            if (EncounterContainer == null) {
                Debug.LogError($"{name}: EncounterContainer is not assigned.");
                return false;
            }

            if (Encounter == null) {
                Debug.LogError($"{name}: Encounter is not assigned.");
                return false;
            }

            ClearRuntimeEncounter();

            runtimeEncounter = Instantiate(Encounter);

            if (IndicatorSprite != null)
                IndicatorSprite.sprite = runtimeEncounter.WorldIndicatorSprite;

            runtimeEncounterVisual = runtimeEncounter.Initialize(EncounterContainer);

            return true;
        }

        public void PunchEncounterVisual(float strength, float duration) {
            if (runtimeEncounterVisual == null || strength <= 0f || duration <= 0f)
                return;

            runtimeEncounterVisual.DOPunchScale(
                Vector3.one * strength,
                duration,
                1,
                0.5f);
        }

        public async UniTask PassedTile() {}

        public async UniTask StoppedOnTile() {
            if (runtimeEncounter == null) {
                Debug.LogError($"{name}: Cannot load an uninitialized encounter.");
                return;
            }

            if (!runtimeEncounter.TriggersWhenStoppedOnTile)
                return;

            await UIEncounterWindow.Instance.LoadEncounter(runtimeEncounter);
        }

    }
}
