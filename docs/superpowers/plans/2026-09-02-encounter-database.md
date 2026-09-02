# Encounter Database Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Generate weighted world encounters once per run, preserve them across World scene reloads, and require no manually assigned tile IDs.

**Architecture:** An `EncounterDatabase` ScriptableObject singleton loaded from `Resources/Databases/EncounterDatabase` owns weighted encounter definitions and stable list-index IDs. `WorldManager` derives route indices by walking `NextTile`, restores or generates a primitive ID list in `GameData`, and assigns encounters before `WorldTile.Start()` initializes them. Character selection clears the layout as part of the existing new-run reset.

**Tech Stack:** Unity 2022.3, C#, Odin Inspector, Unity ScriptableObjects and YAML serialization.

**Spec:** `docs/superpowers/specs/2026-09-02-encounter-database-design.md`

## Global Constraints

- Keep `GameData` primitive and save-friendly; store encounter IDs, not ScriptableObject references.
- Do not add Unity test files or a test assembly.
- Do not edit DuloGames or other vendor code.
- Assigned tile encounters are fixed; only null encounters are generated.
- Generate the route layout once per run and allow natural encounter streaks.
- Use automatic route indices derived from `StartingTile` and `NextTile`.
- Initial weights are Combat `20`, Treasure `70`, Rest `10`.
- Convert all 12 current World tiles to generated encounters.
- Follow the existing database singleton pattern; consumers use `EncounterDatabase.Instance` instead of serialized references.

---

### Task 1: Weighted Encounter Database

**Files:**
- Create: `Assets/_Rogues Path/World/Encounters/EncounterDatabase.cs`
- Create: `Assets/_Rogues Path/World/Encounters/EncounterDatabase.cs.meta`
- Create: `Assets/_Rogues Path/World/Encounters/ScriptableObjects/Resources.meta`
- Create: `Assets/_Rogues Path/World/Encounters/ScriptableObjects/Resources/Databases.meta`
- Create: `Assets/_Rogues Path/World/Encounters/ScriptableObjects/Resources/Databases/EncounterDatabase.asset`
- Create: `Assets/_Rogues Path/World/Encounters/ScriptableObjects/Resources/Databases/EncounterDatabase.asset.meta`

**Interfaces:**
- Consumes: Existing `EncounterData` assets.
- Produces: `EncounterDatabase.Instance`, `bool EncounterDatabase.TryGetByID(int id, out EncounterData encounter)`, and `bool EncounterDatabase.TryGetRandomID(out int id)`.

- [ ] **Step 1: Add the database types and weighted selection**

Create `EncounterDatabase.cs` with this behavior:

```csharp
using System;
using System.Collections.Generic;
using _Rogues_Path._Game;
using UnityEngine;

namespace _Rogues_Path.World.Encounters {
    [Serializable]
    public class WeightedEncounter {
        public EncounterData Encounter;
        [Min(1)] public int Weight = 1;
    }

    [CreateAssetMenu(fileName = nameof(EncounterDatabase), menuName = Game.Name + "/Data/" + nameof(EncounterDatabase))]
    public class EncounterDatabase : ScriptableObject {
        private static EncounterDatabase m_Instance;

        public static EncounterDatabase Instance {
            get {
                if (m_Instance == null)
                    m_Instance = Resources.Load<EncounterDatabase>("Databases/EncounterDatabase");

                return m_Instance;
            }
        }

        [SerializeField] private List<WeightedEncounter> encounters = new();

        public static bool TryGetByID(int id, out EncounterData encounter) {
            encounter = null;

            if (Instance == null || id < 0 || id >= Instance.encounters.Count)
                return false;

            encounter = Instance.encounters[id]?.Encounter;
            return encounter != null;
        }

        public static bool TryGetRandomID(out int id) {
            id = -1;
            int totalWeight = 0;

            if (Instance == null)
                return false;

            foreach (WeightedEncounter entry in Instance.encounters) {
                if (entry?.Encounter != null && entry.Weight > 0)
                    totalWeight += entry.Weight;
            }

            if (totalWeight <= 0) {
                Debug.LogError($"{Instance.name} contains no valid weighted encounters.");
                return false;
            }

            int roll = UnityEngine.Random.Range(0, totalWeight);

            for (int i = 0; i < Instance.encounters.Count; i++) {
                WeightedEncounter entry = Instance.encounters[i];

                if (entry?.Encounter == null || entry.Weight <= 0)
                    continue;

                if (roll < entry.Weight) {
                    id = i;
                    return true;
                }

                roll -= entry.Weight;
            }

            return false;
        }
    }
}
```

- [ ] **Step 2: Create Unity metadata and the configured database asset**

Create `EncounterDatabase.cs.meta`:

```yaml
fileFormatVersion: 2
guid: eeb17b0abc14c8418e0e8e7d484fd695
timeCreated: 1788384000
```

Create `ScriptableObjects/Resources.meta`:

```yaml
fileFormatVersion: 2
guid: 8bcdc9cf7b49da41c53433cbd75bf4df
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData:
  assetBundleName:
  assetBundleVariant:
```

Create `ScriptableObjects/Resources/Databases.meta`:

```yaml
fileFormatVersion: 2
guid: 98e7d7772f48f1db039f9c5709b8990f
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData:
  assetBundleName:
  assetBundleVariant:
```

Create `EncounterDatabase.asset.meta` beneath `ScriptableObjects/Resources/Databases`:

```yaml
fileFormatVersion: 2
guid: 0d65aa4d6372a0246afec31fce31a824
NativeFormatImporter:
  externalObjects: {}
  mainObjectFileID: 11400000
  userData:
  assetBundleName:
  assetBundleVariant:
```

Create `EncounterDatabase.asset` in the same `Resources/Databases` directory, reference the script GUID above, and keep the entries in this exact order so list indices remain stable:

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: eeb17b0abc14c8418e0e8e7d484fd695, type: 3}
  m_Name: EncounterDatabase
  m_EditorClassIdentifier:
encounters:
- Encounter: {fileID: 11400000, guid: d5ab202405ebe5c459c9817a55ce1966, type: 2}
  Weight: 20
- Encounter: {fileID: 11400000, guid: ba335cacf6344bd18a48b12d92396520, type: 2}
  Weight: 70
- Encounter: {fileID: 11400000, guid: 452a47a1b6fb4e5eb5fd1a3ae0ff1745, type: 2}
  Weight: 10
```

- [ ] **Step 3: Verify database source and serialization**

Run:

```bash
git diff --check
rg -n "TryGetByID|TryGetRandomID|Weight: (20|70|10)" \
  "Assets/_Rogues Path/World/Encounters/EncounterDatabase.cs" \
  "Assets/_Rogues Path/World/Encounters/ScriptableObjects/Resources/Databases/EncounterDatabase.asset"
```

Expected: no whitespace errors; both methods and all three weights are present.

- [ ] **Step 4: Commit the database**

```bash
git add "Assets/_Rogues Path/World/Encounters/EncounterDatabase.cs" \
  "Assets/_Rogues Path/World/Encounters/EncounterDatabase.cs.meta" \
  "Assets/_Rogues Path/World/Encounters/ScriptableObjects/Resources.meta" \
  "Assets/_Rogues Path/World/Encounters/ScriptableObjects/Resources/Databases.meta" \
  "Assets/_Rogues Path/World/Encounters/ScriptableObjects/Resources/Databases/EncounterDatabase.asset" \
  "Assets/_Rogues Path/World/Encounters/ScriptableObjects/Resources/Databases/EncounterDatabase.asset.meta"
git commit -m "Add weighted encounter database"
```

### Task 2: Persist the Per-Run Encounter Layout

**Files:**
- Modify: `Assets/_Rogues Path/_Game/Scripts/GameData.cs`
- Modify: `Assets/_Rogues Path/CharacterSelection/Scripts/CharacterSelectionManager.cs`

**Interfaces:**
- Consumes: Existing `Game.Instance` run state.
- Produces: `Game.WorldEncounterOrder`, a primitive `List<int>` cleared by `LockInCharacter()`.

- [ ] **Step 1: Add the saved encounter list**

Add alongside the other `[FoldoutGroup("Data")]` fields in `GameData.cs`:

```csharp
[FoldoutGroup("Data")] public List<int> WorldEncounterOrder = new();
```

- [ ] **Step 2: Clear the layout when a new run starts**

Add immediately after `Game.Instance.PlayerCurrentHealth = -1f;` in `CharacterSelectionManager.LockInCharacter()`:

```csharp
Game.Instance.WorldEncounterOrder.Clear();
```

- [ ] **Step 3: Verify the primitive state and reset**

Run:

```bash
git diff --check
rg -n "WorldEncounterOrder" \
  "Assets/_Rogues Path/_Game/Scripts/GameData.cs" \
  "Assets/_Rogues Path/CharacterSelection/Scripts/CharacterSelectionManager.cs"
```

Expected: one declaration and one new-run clear call.

- [ ] **Step 4: Commit layout persistence**

```bash
git add "Assets/_Rogues Path/_Game/Scripts/GameData.cs" \
  "Assets/_Rogues Path/CharacterSelection/Scripts/CharacterSelectionManager.cs"
git commit -m "Persist world encounter layout"
```

### Task 3: Generate and Restore Encounters by Route Index

**Files:**
- Modify: `Assets/_Rogues Path/World/WorldManager.cs`

**Interfaces:**
- Consumes: `EncounterDatabase.Instance`, `EncounterDatabase.TryGetByID`, `EncounterDatabase.TryGetRandomID`, `Game.WorldEncounterOrder`, `WorldTile.NextTile`, and `WorldTile.Encounter`.
- Produces: `void InitializeEncounters()` and `List<WorldTile> BuildRoute()` called before tile `Start()` methods.

- [ ] **Step 1: Import the encounter namespace**

Add this import to `WorldManager.cs`:

```csharp
using _Rogues_Path.World.Encounters;
```

- [ ] **Step 2: Initialize encounters before constructing the player**

Make `InitializeEncounters();` the first statement of `WorldManager.Awake()`.

- [ ] **Step 3: Implement route traversal**

Add this method to `WorldManager`:

```csharp
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
```

- [ ] **Step 4: Implement generation and restoration**

Add this method to `WorldManager`:

```csharp
private void InitializeEncounters() {
    if (EncounterDatabase.Instance == null) {
        Debug.LogError("Resources/Databases/EncounterDatabase could not be loaded.");
        return;
    }

    List<WorldTile> route = BuildRoute();
    List<int> savedLayout = Game.Instance.WorldEncounterOrder;

    if (savedLayout.Count != route.Count) {
        savedLayout.Clear();

        foreach (WorldTile tile in route) {
            if (tile.Encounter != null) {
                savedLayout.Add(-1);
                continue;
            }

            savedLayout.Add(AssignRandomEncounter(tile));
        }

        return;
    }

    for (int i = 0; i < route.Count; i++) {
        WorldTile tile = route[i];

        if (tile.Encounter != null) {
            savedLayout[i] = -1;
            continue;
        }

        if (EncounterDatabase.TryGetByID(savedLayout[i], out EncounterData savedEncounter)) {
            tile.Encounter = savedEncounter;
            continue;
        }

        savedLayout[i] = AssignRandomEncounter(tile);
    }

    int AssignRandomEncounter(WorldTile tile) {
        if (!EncounterDatabase.TryGetRandomID(out int encounterID) ||
            !EncounterDatabase.TryGetByID(encounterID, out EncounterData encounter)) {

            Debug.LogError($"Failed to generate an encounter for {tile.name}.");
            return -1;
        }

        tile.Encounter = encounter;
        return encounterID;
    }
}
```

- [ ] **Step 5: Verify the route and restore paths are present**

Run:

```bash
git diff --check
rg -n "InitializeEncounters|BuildRoute|visited.Add|WorldEncounterOrder|TryGetRandomID|TryGetByID" \
  "Assets/_Rogues Path/World/WorldManager.cs"
```

Expected: initialization is called from `Awake()`, route traversal has cycle detection, and both restore and random-generation paths exist.

- [ ] **Step 6: Commit world initialization**

```bash
git add "Assets/_Rogues Path/World/WorldManager.cs"
git commit -m "Generate stable weighted world encounters"
```

### Task 4: Configure the World Scene and Verify the Feature

**Files:**
- Modify: `Assets/_Rogues Path/Scenes/World.unity`

**Interfaces:**
- Consumes: The new `EncounterDatabase.asset` through `EncounterDatabase.Instance`.
- Produces: A scene where all 12 current tiles are database-driven.

- [ ] **Step 1: Clear all current encounter overrides**

For each of the 12 `WorldTile` prefab-instance modifications in `World.unity`, keep the `propertyPath: Encounter` override but replace its object reference with:

```yaml
objectReference: {fileID: 0}
```

- [ ] **Step 2: Verify singleton loading and exact conversion count**

Run a source check that confirms `EncounterDatabase.Instance` loads `Databases/EncounterDatabase`, the asset exists beneath a `Resources/Databases` directory, and all 12 `propertyPath: Encounter` overrides now have null object references. Also run:

```bash
git diff --check
git diff --stat
git status --short
```

Expected: no whitespace errors; the expected source, metadata, asset, state, manager, character-selection, scene, spec, and plan files are the only changes in the feature history.

- [ ] **Step 3: Perform Unity Editor verification**

In Unity:

1. Start a new character and enter World.
2. Confirm all 12 tiles display generated encounter visuals.
3. Confirm Treasure is common under the 20/70/10 temporary weights.
4. Complete one Treasure encounter, continue to a second Treasure encounter, and reproduce the pending Select-button bug.
5. Enter Combat, return to World, and confirm the encounter types and positions are unchanged.
6. Start another new character and confirm a fresh layout is generated.
7. Confirm the closed `NextTile` loop initializes without hanging.

- [ ] **Step 4: Commit scene configuration**

```bash
git add "Assets/_Rogues Path/Scenes/World.unity"
git commit -m "Configure weighted world encounters"
```

- [ ] **Step 5: Request publication approval**

Show the user each changed file and its behavior. Do not publish the implementation commits to `Encounters` until the user explicitly approves the complete payload.
