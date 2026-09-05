using System.Collections.Generic;
using System.Linq;
using _Rogues_Path._Game;
using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.Pawns.Scripts;
using _Rogues_Path.UI.CharacterScreen;
using _Rogues_Path.UI.MenuBar;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using _Rogues_Path.World.Encounters;
using Assets.HeroEditor4D.Common.Scripts.Enums;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Michsky.UI.MTP;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using _Rogues_Path.Crafting;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace _Rogues_Path.World {
    public class WorldManager : Singleton<WorldManager> {
        [FoldoutGroup("Settings")]
        [FoldoutGroup("Settings/Movement"), SerializeField] private float MovementJump = 1;
        [FoldoutGroup("Settings/Movement"), SerializeField] private float MovementDuration = 1f;

        [FoldoutGroup("Settings/Dice"), SerializeField] private int DiceCount = 2;
        [FoldoutGroup("Settings/Dice"), SerializeField] private float DieDropHeight = 4f;
        [FoldoutGroup("Settings/Dice"), SerializeField] private float DieAngularVelocityMultiplier = 90f;
        [FoldoutGroup("Settings/Dice"), SerializeField] private float DieLifetime = 5f;
        [FoldoutGroup("Settings/Dice"), SerializeField] private float DieBufferCoefficient = 0.8f;
        [FoldoutGroup("Settings/Dice"), SerializeField] private MeshCollider DiePlaneCollider;

        [FoldoutGroup("References"), SerializeField] private WorldTile StartingTile;
        [FoldoutGroup("References"), SerializeField] private Die DiePrefab;
        [FoldoutGroup("References"), SerializeField] private Button MoveButton;
        [FoldoutGroup("References"), SerializeField] private StyleManager DiceRollAnnouncer;
        [FoldoutGroup("References"), SerializeField] private EquipmentModifierDatabase ModifierDatabase;
        [FoldoutGroup("References"), SerializeField] private UICharacterScreen CharacterScreen;
        [FoldoutGroup("References"), SerializeField] private UIMenuBar MenuBar;
        [FoldoutGroup("Debug"), SerializeField] private Pawn PlayerPawn;
        [FoldoutGroup("Debug"), SerializeField] private WorldTile currentTile;

        private List<WorldTile> route;
        private readonly List<int> randomEncounterIndexes = new();
        private WorldProgressionSettings progressionSettings;

        private void OnEnable() {
            EventBus.SubscribeTo<EquipmentEquippedEvent>(EquipmentEquippedHandler);
            EventBus.SubscribeTo<EquipmentUnequippedEvent>(EquipmentUnequippedHandler);
            EventBus.SubscribeTo<InventoryChanged>(InventoryChangedHandler);
        }

        private void OnDisable() {
            EventBus.UnsubscribeFrom<EquipmentEquippedEvent>(EquipmentEquippedHandler);
            EventBus.UnsubscribeFrom<EquipmentUnequippedEvent>(EquipmentUnequippedHandler);
            EventBus.UnsubscribeFrom<InventoryChanged>(InventoryChangedHandler);
        }

        private void Start() {
            if (PlayerPawn == null)
                return;

            if (CharacterScreen == null || MenuBar == null) {
                Debug.LogError("World character screen and menu bar must be assigned.");
                return;
            }

            CharacterScreen.SetPlayer(Game.Instance.PlayerData);
            MenuBar.Show();
        }

        private void EquipmentEquippedHandler(ref EquipmentEquippedEvent eventData) {
            if (PlayerPawn == null || CharacterScreen == null
                || eventData.Owner == null || eventData.Owner != CharacterScreen.PreviewPawn
                || eventData.Equipment == null || eventData.Equipment.InstanceData == null)
                return;

            if (!EquipmentDatabase.TryCreateInstance(
                    eventData.Equipment.InstanceData,
                    ModifierDatabase,
                    out EquipmentBase liveEquipment,
                    PlayerPawn.transform)) {

                Debug.LogError($"Failed to mirror {eventData.Equipment.Name} onto the World pawn.");
                return;
            }

            // The preview owns the saved equipment and health changes.
            // Update only the map pawn's model, modifiers, and local health.
            if (!PlayerPawn.TryEquip(liveEquipment, modifyGameState: false)) {
                Destroy(liveEquipment.gameObject);
            }
        }

        private void EquipmentUnequippedHandler(ref EquipmentUnequippedEvent eventData) {
            if (PlayerPawn == null || CharacterScreen == null
                || eventData.Owner == null || eventData.Owner != CharacterScreen.PreviewPawn)
                return;

            if (PlayerPawn.CurrentEquipment.TryGetValue(eventData.EquipType, out EquipmentBase equipment)) {
                PlayerPawn.TryRemoveEquipment(equipment, modifyGameState: false);
            }
        }

        private void InventoryChangedHandler(ref InventoryChanged eventData) {
            if (PlayerPawn != null)
                PlayerPawn.SyncInventoryFromGameState();
        }

        private void Awake() {
            route = BuildRoute();

            if (route.Count == 0) {
                Debug.LogError("World route is empty. StartingTile must be assigned.");
                return;
            }

            progressionSettings = WorldProgressionSettings.Instance;

            if (progressionSettings == null) {
                Debug.LogError(
                    "Resources/Databases/WorldProgressionSettings could not be loaded.");
                MoveButton.interactable = false;
                return;
            }

            if (progressionSettings.TraversalCompleteEncounter == null) {
                Debug.LogError(
                    "WorldProgressionSettings requires a traversal-complete encounter.");
                MoveButton.interactable = false;
                return;
            }

            CaptureRandomEncounterIndexes();

            if (!InitializeEncounters()) {
                MoveButton.interactable = false;
                return;
            }

            RestoreCurrentTile();

            // Initialize player with equipment
            PlayerPawn = Instantiate(Game.Instance.PlayerData.Pawn, currentTile.PawnContainer);
            PlayerPawn.Character.SetDirection(Vector2.down);
            PlayerPawn.SyncInventoryFromGameState();
            PlayerPawn.StatusDisplay = null;

            if (PlayerPawn.StatusDisplay != null) {
                Destroy(PlayerPawn.StatusDisplay.gameObject);
                PlayerPawn.StatusDisplay = null;
            }

            foreach (var kvp in Game.Instance.PlayerEquipment) {
                EquipmentInstanceData instanceData = kvp.Value;

                if (!EquipmentDatabase.TryCreateInstance(instanceData, ModifierDatabase, out EquipmentBase liveEquipment, PlayerPawn.transform)) {

                    Debug.LogError($"Failed to create equipment instance for ID " + $"{instanceData.EquipmentID}.");

                    continue;
                }

                if (!PlayerPawn.TryEquip(liveEquipment, false)) {
                    Debug.LogError($"Failed to restore {PlayerPawn.CharacterName} " + $"with {liveEquipment.Name}.");

                    Destroy(liveEquipment.gameObject);
                }
            }

            PlayerHealthState.Restore(PlayerPawn);
            EventBus.Raise(new RunPawnsChanged { Player = PlayerPawn });
        }

        private void CaptureRandomEncounterIndexes() {
            randomEncounterIndexes.Clear();

            for (int i = 0; i < route.Count; i++) {
                if (route[i] != StartingTile && route[i].Encounter == null)
                    randomEncounterIndexes.Add(i);
            }
        }

        private bool InitializeEncounters() {
            if (EncounterDatabase.Instance == null) {
                Debug.LogError("Resources/Databases/EncounterDatabase could not be loaded.");
                return false;
            }

            if (!TryGetSavedEncounterLayout(out List<int> layout)
                && !TryGenerateEncounterLayout(out layout))
                return false;

            return TryApplyEncounterLayout(layout);
        }

        private bool TryGenerateEncounterLayout(out List<int> layout) {
            layout = Enumerable.Repeat(-1, route.Count).ToList();

            int restCount = progressionSettings.RestEncountersPerGeneration;
            int treasureCount = progressionSettings.TreasureEncountersPerGeneration;
            int requiredNonCombatCount = restCount + treasureCount;

            if (randomEncounterIndexes.Count < requiredNonCombatCount) {
                Debug.LogError(
                    $"World requires {requiredNonCombatCount} random tiles for "
                    + $"{restCount} Rest and {treasureCount} Treasure encounters, "
                    + $"but only {randomEncounterIndexes.Count} exist.");

                return false;
            }

            List<int> encounterBag = new();

            for (int i = 0; i < restCount; i++) {
                if (!EncounterDatabase.TryGetRandomID<RestEncounter>(out int encounterID))
                    return false;

                encounterBag.Add(encounterID);
            }

            for (int i = 0; i < treasureCount; i++) {
                if (!EncounterDatabase.TryGetRandomID<TreasureEncounter>(out int encounterID))
                    return false;

                encounterBag.Add(encounterID);
            }

            while (encounterBag.Count < randomEncounterIndexes.Count) {
                if (!EncounterDatabase.TryGetRandomID<CombatEncounter>(out int encounterID))
                    return false;

                encounterBag.Add(encounterID);
            }

            for (int i = 0; i < encounterBag.Count; i++) {
                int randomIndex = UnityEngine.Random.Range(i, encounterBag.Count);
                (encounterBag[i], encounterBag[randomIndex]) =
                    (encounterBag[randomIndex], encounterBag[i]);
            }

            for (int i = 0; i < randomEncounterIndexes.Count; i++)
                layout[randomEncounterIndexes[i]] = encounterBag[i];

            return true;
        }

        private bool TryGetSavedEncounterLayout(out List<int> layout) {
            layout = null;
            List<int> savedLayout = Game.Instance.WorldEncounterOrder;

            if (savedLayout.Count != route.Count)
                return false;

            HashSet<int> randomIndexes = new(randomEncounterIndexes);
            int restCount = 0;
            int treasureCount = 0;
            int combatCount = 0;

            for (int i = 0; i < route.Count; i++) {
                if (!randomIndexes.Contains(i)) {
                    if (savedLayout[i] != -1)
                        return false;

                    continue;
                }

                if (!EncounterDatabase.TryGetByID(
                        savedLayout[i],
                        out EncounterData encounter))
                    return false;

                switch (encounter) {
                    case RestEncounter:
                        restCount++;
                        break;
                    case TreasureEncounter:
                        treasureCount++;
                        break;
                    case CombatEncounter:
                        combatCount++;
                        break;
                    default:
                        return false;
                }
            }

            if (restCount != progressionSettings.RestEncountersPerGeneration
                || treasureCount != progressionSettings.TreasureEncountersPerGeneration
                || combatCount != randomEncounterIndexes.Count - restCount - treasureCount)
                return false;

            layout = new List<int>(savedLayout);
            return true;
        }

        private bool TryResolveEncounterLayout(
            IReadOnlyList<int> layout,
            out Dictionary<int, EncounterData> resolvedEncounters) {

            resolvedEncounters = new Dictionary<int, EncounterData>();

            if (layout == null || layout.Count != route.Count) {
                Debug.LogError("Encounter layout does not match the World route.");
                return false;
            }

            HashSet<int> randomIndexes = new(randomEncounterIndexes);

            for (int i = 0; i < route.Count; i++) {
                if (!randomIndexes.Contains(i)) {
                    if (layout[i] != -1) {
                        Debug.LogError(
                            $"Authored tile {route[i].name} has generated "
                            + $"encounter ID {layout[i]}.");

                        return false;
                    }

                    continue;
                }

                if (!route[i].CanInitializeEncounter) {
                    Debug.LogError($"{route[i].name}: EncounterContainer is not assigned.");
                    return false;
                }

                if (!EncounterDatabase.TryGetByID(
                        layout[i],
                        out EncounterData encounter)) {

                    Debug.LogError(
                        $"Encounter layout contains invalid ID {layout[i]} "
                        + $"for {route[i].name}.");

                    return false;
                }

                resolvedEncounters.Add(i, encounter);
            }

            return true;
        }

        private bool TryApplyEncounterLayout(IReadOnlyList<int> layout) {
            if (!TryResolveEncounterLayout(layout, out Dictionary<int, EncounterData> resolvedEncounters))
                return false;

            foreach (KeyValuePair<int, EncounterData> resolved in resolvedEncounters) {
                if (!route[resolved.Key].TrySetEncounter(resolved.Value))
                    return false;
            }

            Game.Instance.WorldEncounterOrder.Clear();
            Game.Instance.WorldEncounterOrder.AddRange(layout);

            return true;
        }

        private async UniTask<bool> TryApplyEncounterLayoutAnimated(
            IReadOnlyList<int> layout,
            IReadOnlyDictionary<int, EncounterData> resolvedEncounters) {

            foreach (int routeIndex in randomEncounterIndexes)
                route[routeIndex].ClearRuntimeEncounter();

            for (int i = 0; i < randomEncounterIndexes.Count; i++) {
                int routeIndex = randomEncounterIndexes[i];

                if (!route[routeIndex].TrySetEncounter(resolvedEncounters[routeIndex]))
                    return false;

                route[routeIndex].PunchEncounterVisual(
                    progressionSettings.EncounterPunchScale,
                    progressionSettings.EncounterPunchDuration);

                if (i + 1 < randomEncounterIndexes.Count
                    && progressionSettings.EncounterRevealDelay > 0f) {

                    await UniTask.Delay(
                        Mathf.RoundToInt(
                            progressionSettings.EncounterRevealDelay * 1000f));
                }
            }

            if (progressionSettings.EncounterPunchDuration > 0f) {
                await UniTask.Delay(
                    Mathf.RoundToInt(
                        progressionSettings.EncounterPunchDuration * 1000f));
            }

            Game.Instance.WorldEncounterOrder.Clear();
            Game.Instance.WorldEncounterOrder.AddRange(layout);

            return true;
        }

        private void RestoreCurrentTile() {
            int savedIndex = Game.Instance.CurrentWorldTileIndex;

            if (savedIndex < 0 || savedIndex >= route.Count) {
                Debug.LogWarning($"Saved World tile index {savedIndex} is invalid. Returning to the starting tile.");
                savedIndex = 0;
                Game.Instance.CurrentWorldTileIndex = 0;
            }

            currentTile = route[savedIndex];
        }

        private List<WorldTile> BuildRoute() {
            List<WorldTile> route = new();
            HashSet<WorldTile> visited = new();
            WorldTile tile = StartingTile;

            while (tile != null && visited.Add(tile)) {
                route.Add(tile);
                tile = tile.NextTile;
            }

            return route;
        }

        public void UIMoveButtonPressed() {
            RollDiceAndMove(DiceCount).Forget();
        }

        public async UniTask RollDiceAndMove(int numberOfDice) {
            // Make sure number of dice makes sense, disable button
            Debug.Assert(numberOfDice > 0, $"Can't roll {numberOfDice} dice.");
            MoveButton.interactable = false;

            // Roll the dice, await results then total their values
            var diceRolls = await RollDice(numberOfDice);
            var total = diceRolls.Sum();

            // Update Dice Roll Announcer
            DiceRollAnnouncer.textItems[0].text = "Rolls:";
            DiceRollAnnouncer.textItems[1].text = diceRolls.ToCommaDelimitedString();
            DiceRollAnnouncer.textItems[2].text = $"Total: {total}";
            DiceRollAnnouncer.Play();

            // Move 'total' tiles, waiting for Passed/StoppedOnTile on the way
            for (int i = 0; i < total; i++) {
                await MoveToNextTile();

                if (i + 1 < total) {
                    await currentTile.PassedTile();
                }
                else {
                    await currentTile.StoppedOnTile();
                }
            }

            if (this == null)
                return;

            MoveButton.interactable = true;
        }

        public float HealPlayer(float healthPercentage) {
            return PlayerHealthState.Heal(PlayerPawn, healthPercentage);
        }

        private async UniTask MoveToNextTile() {
            WorldTile previousTile = currentTile;
            Vector3 movementDirection = currentTile.NextTile.transform.position - currentTile.transform.position;

            PlayerPawn.Character.SetDirection(GetFacingDirectionFromMovementDirection(movementDirection));
            PlayerPawn.animationManager.SetState(CharacterState.Jump);

            Tween tween = PlayerPawn.transform.DOJump(currentTile.NextTile.PawnContainer.transform.position, MovementJump, 1, MovementDuration, false);
            await tween.AsyncWaitForCompletion();

            currentTile = currentTile.NextTile;
            SaveCurrentTile();
            PlayerPawn.transform.SetParent(currentTile.PawnContainer);
            PlayerPawn.animationManager.SetState(CharacterState.Idle);

            EventBus.Raise(new TileTraversed { Player = PlayerPawn });

            if (previousTile == route[route.Count - 1]
                && currentTile == StartingTile) {

                await CompleteTraversal();
            }

            Vector2 GetFacingDirectionFromMovementDirection(Vector3 direction) {


                Vector3[] directions = {
                    Vector3.forward,
                    Vector3.back,
                    Vector3.left,
                    Vector3.right
                };

                Vector2[] facingDirections = {
                    Vector2.up,
                    Vector2.down,
                    Vector2.left,
                    Vector2.right
                };

                int closestIndex = 0;
                float bestDot = Vector3.Dot(direction, directions[0]);

                for (int i = 1; i < directions.Length; i++) {
                    float dot = Vector3.Dot(direction, directions[i]);

                    if (dot > bestDot) {
                        bestDot = dot;
                        closestIndex = i;
                    }
                }

                Vector2 facingDirection = facingDirections[closestIndex];

                if (facingDirection == Vector2.left) {
                    facingDirection = Vector2.right;
                }
                else if (facingDirection == Vector2.right) {
                    facingDirection = Vector2.left;
                }

                return facingDirection;
            }
        }

        private async UniTask CompleteTraversal() {
            if (!TryGenerateEncounterLayout(out List<int> layout)
                || !TryResolveEncounterLayout(
                    layout,
                    out Dictionary<int, EncounterData> resolvedEncounters)) {

                return;
            }

            Game.Instance.CompletedWorldTraversals++;
            await UIEncounterWindow.Instance.LoadEncounter(
                progressionSettings.TraversalCompleteEncounter);

            if (this == null)
                return;

            await TryApplyEncounterLayoutAnimated(layout, resolvedEncounters);
        }

        private void SaveCurrentTile() {
            int currentIndex = route.IndexOf(currentTile);

            if (currentIndex < 0) {
                Debug.LogError($"{currentTile.name} is not part of the current World route.");
                return;
            }

            Game.Instance.CurrentWorldTileIndex = currentIndex;
        }

        private async UniTask<List<int>> RollDice(int numberOfDice) {
            // Keeping a list of RollDie tasks for a UniTask.WhenAll call
            List<UniTask<int>> diceRollTasks = new();

            // Collect tasks
            for (int i = 0; i < numberOfDice; i++) {
                UniTask<int> dieRollTask = RollDie();
                diceRollTasks.Add(dieRollTask);
            }

            // Execute and wait for all tasks to fill array and return it
            var diceRollValues = await UniTask.WhenAll(diceRollTasks);
            return diceRollValues.ToList();
        }

        private async UniTask<int> RollDie() {
            // Plane the die drops on
            var planeExtents = DiePlaneCollider.bounds.extents;

            // Get a 'suitable' random position within planeExtents at dropHeight height
            var dropPosition = new Vector3(
                UnityEngine.Random.Range(-planeExtents.x, planeExtents.x) * DieBufferCoefficient,
                DieDropHeight,
                UnityEngine.Random.Range(-planeExtents.y, planeExtents.y) * DieBufferCoefficient);

            // Instantiate with random rotation
            var die = Instantiate(DiePrefab, dropPosition, UnityEngine.Random.rotation);

            // Give the die a random spin
            die.RigidBody.angularVelocity = UnityEngine.Random.rotation.eulerAngles * DieAngularVelocityMultiplier;

            // Wait until ReadDie isn't null, meaning it has stopped
            await UniTask.WaitUntil(() => die.ReadDie() != null);
            Destroy(die.gameObject, DieLifetime);
            return die.ReadDie()!.Value;
        }
    }
}
