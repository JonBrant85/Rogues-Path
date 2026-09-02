# Encounter Database Design

## Goal

Replace hand-assigned random world encounters with a weighted, data-driven system. Encounters are generated once per run, remain stable when the World scene reloads, and require no manually assigned tile IDs.

## Data Model

### EncounterDatabase

`EncounterDatabase` is a ScriptableObject singleton following the existing `OrbDatabase` and `EquipmentDatabase` pattern. `EncounterDatabase.Instance` loads the asset from `Resources/Databases/EncounterDatabase`. The database contains an ordered list of entries. Each entry has:

- An `EncounterData` reference.
- A positive integer weight.

The entry's list index is its persistent encounter ID. The database exposes weighted selection and ID lookup methods. Null encounters and entries with weights less than one are excluded from selection.

The initial `EncounterDatabase.asset` configuration is:

| Encounter | Weight |
| --- | ---: |
| A lone bunny (Combat) | 20 |
| Treasure | 70 |
| Rest | 10 |

Rolls are independent. Consecutive encounters of the same type are allowed.

### GameData

`GameData.cs` receives a primitive `List<int> WorldEncounterOrder`.

Each list position corresponds to the automatically derived route index of a `WorldTile`:

- `-1` means the tile has a hand-authored encounter and must not be changed.
- A non-negative value is an `EncounterDatabase` entry ID for a generated encounter.

This keeps persistent state free of ScriptableObject references.

## Route Indexing

`WorldManager` builds the route by walking `NextTile`, beginning at `StartingTile`. The starting tile has route index `0`, its successor has index `1`, and so on.

Traversal stops when it reaches a null `NextTile` or a tile it has already visited. This supports both a linear route and a closed board loop without an infinite traversal.

The indices are derived at runtime and are not serialized onto individual tiles. Inserting or reordering tiles invalidates an in-progress run's layout, which is acceptable during development. Save-version migration can address this when permanent saves are introduced.

## Generation and Restoration Flow

`WorldManager.Awake()` prepares encounters before any `WorldTile.Start()` method runs:

1. Build the ordered route from `StartingTile`.
2. Check whether `Game.Instance.WorldEncounterOrder` has the same length as the route.
3. If the lengths differ, regenerate the entire layout.
4. For each route tile:
   - If `WorldTile.Encounter` is already assigned in the scene, preserve it and store `-1` at that route index.
   - If the tile is unassigned and has a valid saved database ID, restore that encounter.
   - If the tile is unassigned and its saved ID is invalid, roll a replacement and update that list position.
5. `WorldTile.Start()` clones and initializes the assigned encounter using its existing behavior.

`WorldManager` accesses `EncounterDatabase.Instance`; the database does not require a scene reference.

All 12 current world-tile encounter overrides will be cleared so the database generates them. Any encounter assigned directly to a tile in the future is intentionally hand-authored and remains exempt from weighted generation.

## New Run Reset

`CharacterSelectionManager.LockInCharacter()` currently defines the beginning of a new run by clearing equipment, inventory, and saved health. It will also clear `WorldEncounterOrder`, ensuring a newly selected character receives a fresh encounter layout.

If character selection later becomes separate from starting a run, all run resets should move together into an explicit `Game.StartNewRun()` method.

## Error Handling

- A missing `Resources/Databases/EncounterDatabase` asset logs an error and stops generated encounter assignment.
- A database with no valid positive-weight entries logs an error and cannot generate encounters.
- Null route tiles are not possible while following `NextTile`; traversal ends at null.
- Invalid saved encounter IDs are rerolled individually when the route length still matches.
- A route-length mismatch regenerates the complete layout.

## Files

- `Assets/_Rogues Path/World/Encounters/EncounterDatabase.cs` — database types, weighted selection, and ID lookup.
- `Assets/_Rogues Path/World/Encounters/ScriptableObjects/Resources/Databases/EncounterDatabase.asset` — singleton weighted encounter configuration.
- `Assets/_Rogues Path/World/WorldManager.cs` — route traversal and encounter generation/restoration.
- `Assets/_Rogues Path/_Game/Scripts/GameData.cs` — saved encounter ID list.
- `Assets/_Rogues Path/CharacterSelection/Scripts/CharacterSelectionManager.cs` — new-run layout reset.
- `Assets/_Rogues Path/Scenes/World.unity` — removal of all 12 current encounter overrides.

## Verification

No Unity test assembly will be added. Verification consists of static source and Unity serialization checks here, followed by these Editor checks:

1. Start a new character and confirm formerly null tiles receive encounters.
2. Confirm hand-authored encounters remain unchanged.
3. Confirm Treasure appears substantially more often with the temporary 20/70/10 weights.
4. Enter Combat, return to World, and confirm the remaining encounter layout is unchanged.
5. Start another new character and confirm a new layout is generated.
6. Confirm a closed `NextTile` loop does not hang during initialization.

## Deferred Work

This change does not implement player tile-position restoration, equipment-quality weighting, maximum-health gain behavior, or the second-Treasure selection bug fix. It only makes repeated Treasure encounters practical to reproduce and provides the encounter-layout foundation needed by tile-position persistence.
