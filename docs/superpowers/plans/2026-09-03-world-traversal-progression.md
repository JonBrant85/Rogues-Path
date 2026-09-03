# World Traversal Progression Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Turn each completed circuit of the World board into a persistent endurance tier that rerolls random encounters and linearly scales newly instantiated enemies.

**Architecture:** `GameData` stores only the completed traversal count. A singleton `WorldProgressionSettings` ScriptableObject owns tunable scaling rates and exact encounter counts, `WorldManager` owns traversal detection and layout generation, `WorldTile` owns safe runtime encounter replacement, and `EnemyTraversalScaler` mutates only fresh runtime enemy clones.

**Tech Stack:** Unity 2022.3, C#, UniTask, Odin Inspector, Michsky Motion Titles Pack, existing Rogue's Path singleton/database patterns.

**Spec:** `docs/superpowers/specs/2026-09-03-world-traversal-progression-design.md`

## Global Constraints

- Do not modify DuloGames, Michsky, HeroEditor, or other vendor code.
- Do not modify the newly pushed Skeleton enemy files; Skeleton diagnosis is the next separate task.
- Do not add `#region` or `#endregion` blocks.
- Keep `GameData` save-friendly: traversal state is a primitive `int`.
- Traversal zero is unscaled; health gains 20% and every other enemy base stat gains 10% per completed traversal.
- Scaling is linear, uncapped, and applies once to fresh runtime enemy clones only.
- Only randomly generated World tiles reroll; authored encounters remain fixed.
- Every generated layout contains exactly two Rest encounters, exactly one Treasure encounter, and Combat on all remaining random tiles.
- Loot scaling is excluded from this implementation.
- Do not add Unity test assemblies or test files. This project currently has no project test assembly, and the user explicitly declined test-file work; use focused static checks and Unity playtesting instead.
- Keep each implementation commit local until the user reviews the changes and explicitly approves publication.

---

### Task 1: Persistent traversal state and progression settings

**Files:**
- Create: `Assets/_Rogues Path/World/WorldProgressionSettings.cs`
- Create: `Assets/_Rogues Path/World/WorldProgressionSettings.cs.meta`
- Create: `Assets/Resources/Databases/WorldProgressionSettings.asset`
- Create: `Assets/Resources/Databases/WorldProgressionSettings.asset.meta`
- Modify: `Assets/_Rogues Path/_Game/Scripts/GameData.cs`
- Modify: `Assets/_Rogues Path/CharacterSelection/Scripts/CharacterSelectionManager.cs`

**Interfaces:**
- Produces: `WorldProgressionSettings.Instance`
- Produces: `GetEnemyHealthMultiplier(int completedTraversals)`
- Produces: `GetEnemyStatMultiplier(int completedTraversals)`
- Produces: `Game.Instance.CompletedWorldTraversals`

- [ ] **Step 1: Add the persistent counter**

Add beside the other World state in `GameData.cs`:

```csharp
[FoldoutGroup("Data")] public int CompletedWorldTraversals;
```

In `CharacterSelectionManager.LockInCharacter()`, reset it with the other new-run state:

```csharp
Game.Instance.WorldEncounterOrder.Clear();
Game.Instance.CurrentWorldTileIndex = 0;
Game.Instance.CompletedWorldTraversals = 0;
```

- [ ] **Step 2: Create the settings singleton**

Create `WorldProgressionSettings.cs`:

```csharp
using _Rogues_Path._Game;
using UnityEngine;

namespace _Rogues_Path.World {
    [CreateAssetMenu(
        menuName = Game.Name + "/" + nameof(WorldProgressionSettings),
        fileName = nameof(WorldProgressionSettings))]
    public class WorldProgressionSettings : ScriptableObject {
        private static WorldProgressionSettings m_Instance;

        public static WorldProgressionSettings Instance {
            get {
                if (m_Instance == null) {
                    m_Instance = Resources.Load<WorldProgressionSettings>(
                        "Databases/WorldProgressionSettings");
                }

                return m_Instance;
            }
        }

        [Min(0f)] public float EnemyHealthPerTraversal = 0.20f;
        [Min(0f)] public float EnemyStatPerTraversal = 0.10f;
        [Min(0)] public int RestEncountersPerGeneration = 2;
        [Min(0)] public int TreasureEncountersPerGeneration = 1;

        public float GetEnemyHealthMultiplier(int completedTraversals) {
            return 1f + Mathf.Max(0, completedTraversals)
                * EnemyHealthPerTraversal;
        }

        public float GetEnemyStatMultiplier(int completedTraversals) {
            return 1f + Mathf.Max(0, completedTraversals)
                * EnemyStatPerTraversal;
        }
    }
}
```

Create its Unity metadata, then create `WorldProgressionSettings.asset` under the existing `Assets/Resources/Databases` folder with the four serialized values `0.2`, `0.1`, `2`, and `1`. The asset's `m_Script` GUID must exactly match `WorldProgressionSettings.cs.meta`.

- [ ] **Step 3: Run static verification**

Run:

```bash
rg -n "CompletedWorldTraversals|EnemyHealthPerTraversal|EnemyStatPerTraversal|RestEncountersPerGeneration|TreasureEncountersPerGeneration"   "Assets/_Rogues Path/_Game/Scripts/GameData.cs"   "Assets/_Rogues Path/CharacterSelection/Scripts/CharacterSelectionManager.cs"   "Assets/_Rogues Path/World/WorldProgressionSettings.cs"   "Assets/Resources/Databases/WorldProgressionSettings.asset"
git diff --check
```

Expected: the counter appears in data and reset code; all four settings appear in code and the asset; `git diff --check` exits zero.

- [ ] **Step 4: Commit locally**

```bash
git add   "Assets/_Rogues Path/_Game/Scripts/GameData.cs"   "Assets/_Rogues Path/CharacterSelection/Scripts/CharacterSelectionManager.cs"   "Assets/_Rogues Path/World/WorldProgressionSettings.cs"   "Assets/_Rogues Path/World/WorldProgressionSettings.cs.meta"   "Assets/Resources/Databases/WorldProgressionSettings.asset"   "Assets/Resources/Databases/WorldProgressionSettings.asset.meta"
git commit -m "Add world traversal progression settings"
```

---

### Task 2: Weighted encounter selection by category

**Files:**
- Modify: `Assets/_Rogues Path/World/Encounters/EncounterDatabase.cs`

**Interfaces:**
- Consumes: existing `WeightedEncounter` entries and their weights.
- Produces: `TryGetRandomID<TEncounter>(out int id) where TEncounter : EncounterData`
- Preserves: `TryGetRandomID(out int id)`

- [ ] **Step 1: Add typed weighted selection**

Replace the body of the current unfiltered method with a call to the generic implementation and add the generic overload:

```csharp
public static bool TryGetRandomID(out int id) {
    return TryGetRandomID<EncounterData>(out id);
}

public static bool TryGetRandomID<TEncounter>(out int id)
    where TEncounter : EncounterData {

    id = -1;

    if (Instance == null)
        return false;

    int totalWeight = 0;

    foreach (WeightedEncounter entry in Instance.encounters) {
        if (entry?.Encounter is TEncounter && entry.Weight > 0)
            totalWeight += entry.Weight;
    }

    if (totalWeight <= 0) {
        Debug.LogError(
            $"{Instance.name} contains no valid weighted "
            + $"{typeof(TEncounter).Name} encounters.");

        return false;
    }

    int roll = UnityEngine.Random.Range(0, totalWeight);

    for (int i = 0; i < Instance.encounters.Count; i++) {
        WeightedEncounter entry = Instance.encounters[i];

        if (entry?.Encounter is not TEncounter || entry.Weight <= 0)
            continue;

        if (roll < entry.Weight) {
            id = i;
            return true;
        }

        roll -= entry.Weight;
    }

    return false;
}
```

This preserves existing callers while allowing the generator to request Rest, Treasure, and Combat independently.

- [ ] **Step 2: Run static verification**

Run:

```bash
rg -n "TryGetRandomID<TEncounter>|is TEncounter|is not TEncounter|typeof\(TEncounter\)"   "Assets/_Rogues Path/World/Encounters/EncounterDatabase.cs"
git diff --check
```

Expected: the generic method filters both weight accumulation and selection; the unfiltered overload remains available.

- [ ] **Step 3: Commit locally**

```bash
git add "Assets/_Rogues Path/World/Encounters/EncounterDatabase.cs"
git commit -m "Add typed weighted encounter selection"
```

---

### Task 3: Safe runtime encounter replacement

**Files:**
- Modify: `Assets/_Rogues Path/World/WorldTile.cs`

**Interfaces:**
- Produces: `bool CanInitializeEncounter`
- Produces: `bool TrySetEncounter(EncounterData encounter)`
- Maintains: public `Encounter` as the assigned template.
- Maintains: private `runtimeEncounter` as the cloned encounter used by `StoppedOnTile()`.

- [ ] **Step 1: Separate the template from the runtime clone**

Add:

```csharp
private EncounterData runtimeEncounter;
private bool hasStarted;

public bool CanInitializeEncounter => EncounterContainer != null;
```

Replace `Start()` with:

```csharp
private void Start() {
    hasStarted = true;

    if (!TryInitializeEncounter())
        Debug.LogError($"{name}: Failed to initialize encounter.");
}
```

- [ ] **Step 2: Add replacement and initialization methods**

Add:

```csharp
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

private bool TryInitializeEncounter() {
    if (EncounterContainer == null) {
        Debug.LogError($"{name}: EncounterContainer is not assigned.");
        return false;
    }

    if (Encounter == null) {
        Debug.LogError($"{name}: Encounter is not assigned.");
        return false;
    }

    if (runtimeEncounter != null)
        Destroy(runtimeEncounter);

    for (int i = EncounterContainer.childCount - 1; i >= 0; i--) {
        GameObject previousVisual = EncounterContainer.GetChild(i).gameObject;
        previousVisual.SetActive(false);
        Destroy(previousVisual);
    }

    runtimeEncounter = Instantiate(Encounter);

    if (IndicatorSprite != null)
        IndicatorSprite.sprite = runtimeEncounter.WorldIndicatorSprite;

    runtimeEncounter.Initialize(EncounterContainer);

    return true;
}
```

Update `StoppedOnTile()` to use the runtime clone:

```csharp
public async UniTask StoppedOnTile() {
    if (runtimeEncounter == null) {
        Debug.LogError($"{name}: Cannot load an uninitialized encounter.");
        return;
    }

    await UIEncounterWindow.Instance.LoadEncounter(runtimeEncounter);
}
```

- [ ] **Step 3: Run static verification**

Run:

```bash
rg -n "runtimeEncounter|CanInitializeEncounter|TrySetEncounter|TryInitializeEncounter"   "Assets/_Rogues Path/World/WorldTile.cs"
git diff --check
```

Expected: `Encounter` is never overwritten with its clone; replacement clears previous visual children; stopped-on handling uses `runtimeEncounter`.

- [ ] **Step 4: Commit locally**

```bash
git add "Assets/_Rogues Path/World/WorldTile.cs"
git commit -m "Support runtime encounter replacement"
```

---

### Task 4: Exact layout generation and traversal rerolling

**Files:**
- Modify: `Assets/_Rogues Path/World/WorldManager.cs`

**Interfaces:**
- Consumes: `WorldProgressionSettings.Instance`
- Consumes: `EncounterDatabase.TryGetRandomID<TEncounter>()`
- Consumes: `WorldTile.CanInitializeEncounter` and `TrySetEncounter()`
- Produces: exact generated layouts in `Game.Instance.WorldEncounterOrder`
- Produces: traversal detection and `DiceRollAnnouncer` messaging

- [ ] **Step 1: Record random tile indexes before assignment**

Add fields:

```csharp
private readonly List<int> randomEncounterIndexes = new();
private WorldProgressionSettings progressionSettings;
```

After `route = BuildRoute();` and its empty-route guard in `Awake()`, load settings, capture random indexes, and make encounter initialization a required success:

```csharp
progressionSettings = WorldProgressionSettings.Instance;

if (progressionSettings == null) {
    Debug.LogError(
        "Resources/Databases/WorldProgressionSettings could not be loaded.");
    MoveButton.interactable = false;
    return;
}

CaptureRandomEncounterIndexes();

if (!InitializeEncounters()) {
    MoveButton.interactable = false;
    return;
}
```

Add:

```csharp
private void CaptureRandomEncounterIndexes() {
    randomEncounterIndexes.Clear();

    for (int i = 0; i < route.Count; i++) {
        if (route[i].Encounter == null)
            randomEncounterIndexes.Add(i);
    }
}
```

- [ ] **Step 2: Build exact encounter layouts transactionally**

Refactor `InitializeEncounters()` to return `bool`. First attempt to validate and restore `Game.Instance.WorldEncounterOrder`; otherwise generate a complete replacement layout.

Add a generator with these exact category counts:

```csharp
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
        if (!EncounterDatabase.TryGetRandomID<RestEncounter>(
                out int encounterID))
            return false;

        encounterBag.Add(encounterID);
    }

    for (int i = 0; i < treasureCount; i++) {
        if (!EncounterDatabase.TryGetRandomID<TreasureEncounter>(
                out int encounterID))
            return false;

        encounterBag.Add(encounterID);
    }

    while (encounterBag.Count < randomEncounterIndexes.Count) {
        if (!EncounterDatabase.TryGetRandomID<CombatEncounter>(
                out int encounterID))
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
```

- [ ] **Step 3: Validate saved layouts**

Add `TryGetSavedEncounterLayout(out List<int> layout)`. It must return false unless all of these conditions hold:

```csharp
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
        || treasureCount
            != progressionSettings.TreasureEncountersPerGeneration
        || combatCount
            != randomEncounterIndexes.Count - restCount - treasureCount)
        return false;

    layout = new List<int>(savedLayout);
    return true;
}
```

This ensures an older or corrupt layout is regenerated instead of violating the exact composition.

- [ ] **Step 4: Apply complete layouts**

Add `TryApplyEncounterLayout(IReadOnlyList<int> layout)`. Before changing any tile or saved state, it must:

1. Confirm `layout.Count == route.Count`.
2. Confirm every random tile reports `CanInitializeEncounter`.
3. Resolve every random encounter ID to a non-null `EncounterData`.
4. Keep authored indexes at `-1`.

After all validation succeeds, call `TrySetEncounter()` for each random tile. Then replace saved state in one operation:

```csharp
Game.Instance.WorldEncounterOrder.Clear();
Game.Instance.WorldEncounterOrder.AddRange(layout);
```

`InitializeEncounters()` becomes:

```csharp
private bool InitializeEncounters() {
    if (EncounterDatabase.Instance == null) {
        Debug.LogError(
            "Resources/Databases/EncounterDatabase could not be loaded.");
        return false;
    }

    if (!TryGetSavedEncounterLayout(out List<int> layout)
        && !TryGenerateEncounterLayout(out layout))
        return false;

    return TryApplyEncounterLayout(layout);
}
```

- [ ] **Step 5: Detect completed traversals**

In `MoveToNextTile()`, retain the tile being left and check the boundary after movement completes:

```csharp
WorldTile previousTile = currentTile;
Vector3 movementDirection =
    currentTile.NextTile.transform.position - currentTile.transform.position;

// Existing facing, animation, tween, and await remain here.

currentTile = currentTile.NextTile;

if (previousTile == route[route.Count - 1]
    && currentTile == StartingTile) {
    CompleteTraversal();
}

SaveCurrentTile();
PlayerPawn.transform.SetParent(currentTile.PawnContainer);
PlayerPawn.animationManager.SetState(CharacterState.Idle);
```

Add:

```csharp
private void CompleteTraversal() {
    if (!TryGenerateEncounterLayout(out List<int> layout)
        || !TryApplyEncounterLayout(layout))
        return;

    Game.Instance.CompletedWorldTraversals++;
    AnnounceCompletedTraversal();
}
```

A failed generation leaves both the current layout and traversal count unchanged.

- [ ] **Step 6: Reuse DiceRollAnnouncer**

Add:

```csharp
private void AnnounceCompletedTraversal() {
    if (DiceRollAnnouncer == null
        || DiceRollAnnouncer.textItems == null
        || DiceRollAnnouncer.textItems.Count < 3) {

        Debug.LogError(
            "DiceRollAnnouncer requires at least three text items "
            + "to announce a completed traversal.");

        return;
    }

    DiceRollAnnouncer.textItems[0].text = "Traversal Complete";
    DiceRollAnnouncer.textItems[1].text =
        $"Traversal {Game.Instance.CompletedWorldTraversals}";
    DiceRollAnnouncer.textItems[2].text = "Enemies grow stronger!";
    DiceRollAnnouncer.Play();
}
```

- [ ] **Step 7: Run static verification**

Run:

```bash
rg -n "CaptureRandomEncounterIndexes|TryGenerateEncounterLayout|TryGetSavedEncounterLayout|TryApplyEncounterLayout|CompleteTraversal|AnnounceCompletedTraversal"   "Assets/_Rogues Path/World/WorldManager.cs"
rg -n "RestEncounter|TreasureEncounter|CombatEncounter|CompletedWorldTraversals|Traversal Complete"   "Assets/_Rogues Path/World/WorldManager.cs"
git diff --check
```

Expected: every layout path uses the exact typed categories; the counter changes only inside `CompleteTraversal()`; traversal detection compares the final route tile to `StartingTile`.

- [ ] **Step 8: Commit locally**

```bash
git add "Assets/_Rogues Path/World/WorldManager.cs"
git commit -m "Reroll encounters on world traversal"
```

---

### Task 5: Scale fresh runtime enemies

**Files:**
- Create: `Assets/_Rogues Path/World/EnemyTraversalScaler.cs`
- Create: `Assets/_Rogues Path/World/EnemyTraversalScaler.cs.meta`
- Modify: `Assets/_Rogues Path/Combat/CombatManager.cs`

**Interfaces:**
- Consumes: `WorldProgressionSettings` multiplier methods.
- Consumes: `Game.Instance.CompletedWorldTraversals`.
- Produces: `EnemyTraversalScaler.TryApply(Pawn enemy, int completedTraversals, WorldProgressionSettings settings)`.

- [ ] **Step 1: Create the runtime scaler**

Create `EnemyTraversalScaler.cs`:

```csharp
using System.Collections.Generic;
using _Rogues_Path.Pawns.Scripts;
using _Rogues_Path.UI.CharacterScreen;
using Kryz.CharacterStats;
using UnityEngine;

namespace _Rogues_Path.World {
    public static class EnemyTraversalScaler {
        public static bool TryApply(
            Pawn enemy,
            int completedTraversals,
            WorldProgressionSettings settings) {

            if (enemy == null) {
                Debug.LogError("Cannot scale a null enemy.");
                return false;
            }

            if (settings == null) {
                Debug.LogError(
                    "Cannot scale enemy without WorldProgressionSettings.");
                return false;
            }

            if (enemy.Stats == null
                || !enemy.Stats.TryGetValue(
                    enemy.MaxHealthID,
                    out CharacterStat maximumHealth)) {

                Debug.LogError(
                    $"{enemy.CharacterName} has no maximum-health stat.");
                return false;
            }

            float healthMultiplier =
                settings.GetEnemyHealthMultiplier(completedTraversals);
            float statMultiplier =
                settings.GetEnemyStatMultiplier(completedTraversals);

            foreach (KeyValuePair<CharacterStatID, CharacterStat> stat
                     in enemy.Stats) {
                if (stat.Value == null)
                    continue;

                float multiplier = stat.Key == enemy.MaxHealthID
                    ? healthMultiplier
                    : statMultiplier;

                stat.Value.BaseValue *= multiplier;
            }

            enemy.CurrentHealth = maximumHealth.Value;

            return true;
        }
    }
}
```

The scaler changes only runtime `CharacterStat.BaseValue` values. It does not alter the dictionary, stat IDs, PawnData, prefabs, player, or equipment.

- [ ] **Step 2: Apply scaling in CombatManager**

Immediately after:

```csharp
Enemy = Instantiate(randomEnemy.Pawn, EnemyContainer);
```

add:

```csharp
if (!EnemyTraversalScaler.TryApply(
        Enemy,
        Game.Instance.CompletedWorldTraversals,
        WorldProgressionSettings.Instance)) {

    Debug.LogError($"Failed to scale enemy {Enemy.CharacterName}.");
}
```

Add `using _Rogues_Path.World;` to `CombatManager.cs`.

Scaling occurs before facing, timers, and combat processing. The enemy's current health is reset to its scaled maximum by the scaler.

- [ ] **Step 3: Run static verification**

Run:

```bash
rg -n "EnemyTraversalScaler|GetEnemyHealthMultiplier|GetEnemyStatMultiplier|BaseValue \*=|CurrentHealth = maximumHealth.Value"   "Assets/_Rogues Path/World/EnemyTraversalScaler.cs"   "Assets/_Rogues Path/Combat/CombatManager.cs"
git diff --check
```

Expected: one call site immediately follows enemy instantiation; only runtime enemy stats are mutated.

- [ ] **Step 4: Commit locally**

```bash
git add   "Assets/_Rogues Path/World/EnemyTraversalScaler.cs"   "Assets/_Rogues Path/World/EnemyTraversalScaler.cs.meta"   "Assets/_Rogues Path/Combat/CombatManager.cs"
git commit -m "Scale enemies by world traversal"
```

---

### Task 6: Whole-feature verification and handoff

**Files:**
- Inspect all files changed in Tasks 1–5.
- Do not modify Skeleton files or vendor files.

**Interfaces:**
- Verifies the complete traversal progression flow.

- [ ] **Step 1: Run repository checks**

```bash
git diff --check HEAD~5 HEAD
git status --short
git diff --name-only HEAD~5 HEAD
```

Expected: no whitespace errors; worktree clean after commits; changed paths are restricted to the files listed in this plan plus their new Unity metadata and settings asset.

Confirm no changed path begins with:

```text
Assets/ThirdParty/
Assets/_Rogues Path/Pawns/Enemies/Skeleton
```

Use the actual Skeleton paths from the user's latest pushed commit when checking the second exclusion.

- [ ] **Step 2: Verify serialized settings**

Inspect `WorldProgressionSettings.asset` and confirm:

```text
EnemyHealthPerTraversal: 0.2
EnemyStatPerTraversal: 0.1
RestEncountersPerGeneration: 2
TreasureEncountersPerGeneration: 1
```

Confirm its `m_Script` GUID equals the GUID in `WorldProgressionSettings.cs.meta`.

- [ ] **Step 3: Unity compile check**

Open the project in Unity 2022.3 and wait for script compilation.

Expected: Console contains no C# compilation errors. Do not claim compilation success if a Unity executable is unavailable; report static verification separately and hand the compile check to the user.

- [ ] **Step 4: Unity playtest checklist**

Run a fresh character and verify:

1. `CompletedWorldTraversals == 0`.
2. Random tiles contain exactly two Rest and one Treasure encounter; every other random tile is Combat.
3. Any authored encounter remains fixed.
4. Leave and re-enter World before completing a circuit; encounter positions remain unchanged.
5. Cross from the final route tile to `StartingTile`; the counter becomes 1.
6. The DiceRollAnnouncer displays `Traversal Complete`, `Traversal 1`, and `Enemies grow stronger!`.
7. Random tiles reroll immediately; authored tiles do not.
8. If the dice roll continues past `StartingTile`, movement completes normally.
9. Enter Combat on traversal 1; maximum health is 120% of the enemy prefab's base and every other base stat is 110%.
10. Complete additional circuits; traversal 5 produces 200% maximum health and 150% other base stats.
11. Enter multiple combats at the same traversal; values remain identical rather than scaling repeatedly.
12. Start a new character; traversal returns to zero and a new exact-composition board is generated.

- [ ] **Step 5: Present changes for approval**

Show the user every changed file and its behavior, the local commit list, static verification output, and any Unity checks that could not be run. Do not publish gameplay commits until the user explicitly approves.
