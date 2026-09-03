# World Traversal Progression Design

**Date:** 2026-09-03  
**Branch:** `Encounters`

## Goal

Turn the looping World board into an endurance run. Each completed traversal rerolls randomly generated encounters and permanently increases enemy difficulty for the current run. The traversal counter will later drive increased loot drops, but loot scaling is outside this feature.

## Approved behavior

- A traversal completes when the pawn moves from the final route tile onto `StartingTile`.
- The boundary is evaluated for every movement step, including crossings in the middle of a dice roll.
- Every crossing increments the traversal counter once.
- Traversal zero uses unscaled enemies.
- Enemy maximum health gains 20% of its original base value per completed traversal.
- Every other enemy base stat gains 10% of its original base value per completed traversal.
- Scaling is linear, has no cap, and is always calculated from the newly instantiated enemy's original base values.
- Only randomly generated tiles reroll.
- Authored encounters assigned in the World scene remain fixed.
- Every generated layout contains exactly two Rest encounters and exactly one Treasure encounter among its random tiles.
- Every remaining random tile contains a Combat encounter.
- The existing `DiceRollAnnouncer` announces each completed traversal.

## Persistent run state

### `GameData.cs`

Add a save-friendly integer:

```csharp
[FoldoutGroup("Data")] public int CompletedWorldTraversals;
```

The value persists through World, Combat, and Rewards scene changes. `CharacterSelectionManager.LockInCharacter()` resets it to zero when beginning a new run.

`WorldEncounterOrder` continues storing the generated encounter ID for each route index and `-1` for authored encounters. A generated layout remains stable through scene changes until the next traversal is completed.

## Progression settings

### `WorldProgressionSettings.cs`

Add a Rogue's Path-owned singleton ScriptableObject loaded from `Resources/Databases/WorldProgressionSettings`.

Initial serialized settings:

```csharp
[Min(0f)] public float EnemyHealthPerTraversal = 0.20f;
[Min(0f)] public float EnemyStatPerTraversal = 0.10f;
[Min(0)] public int RestEncountersPerGeneration = 2;
[Min(0)] public int TreasureEncountersPerGeneration = 1;
```

This asset is the single Inspector-editable source for the endurance curve. Future loot-scaling settings may be added here without changing the persistent `GameData` structure.

## Traversal detection and flow

### `WorldManager.cs`

At initialization, `WorldManager` builds the route and records which tiles had no authored `Encounter` before assigning anything. Those route indexes are the random encounter positions for the lifetime of that World scene.

For each movement step:

1. Remember the tile being left.
2. Move to `currentTile.NextTile`.
3. If the old tile was the route's final tile and the new tile is `StartingTile`, complete one traversal.
4. Save the current route index.
5. Continue the dice roll and process the final stopped-on encounter normally.

Completing a traversal performs this order:

1. Build a complete replacement encounter layout without modifying the active board.
2. If generation succeeds, increment `CompletedWorldTraversals`.
3. Save and apply the replacement IDs only to random tiles.
4. Refresh those tiles' runtime encounters and visuals.
5. Replay `DiceRollAnnouncer`.

The reroll happens before `StoppedOnTile()`. If movement ends on `StartingTile`, its newly assigned random encounter is the encounter that runs.

## Exact encounter generation

### `EncounterDatabase.cs`

Add weighted selection filtered by encounter subtype. The database can request a weighted ID for `RestEncounter`, `TreasureEncounter`, or `CombatEncounter`. Existing entry weights are respected within the requested category, allowing multiple variants later.

### `WorldManager.cs`

Generation uses a temporary encounter bag:

1. Validate that the number of random tiles can contain the required Rest and Treasure encounters.
2. Add exactly two weighted Rest encounter IDs.
3. Add exactly one weighted Treasure encounter ID.
4. Add weighted Combat encounter IDs until the bag count equals the random tile count.
5. Shuffle the complete bag.
6. Map the shuffled IDs to the recorded random route indexes.
7. Store `-1` at authored route indexes.

Authored encounters do not count toward the exact generated quantities. For example, an authored Rest tile may exist in addition to the two generated Rest tiles.

The same generator is used for the initial board and every traversal reroll.

## Runtime encounter replacement

### `WorldTile.cs`

Centralize encounter initialization and replacement in a method owned by `WorldTile`.

Replacing an encounter:

- Deactivates and destroys the previous runtime encounter clone.
- Deactivates and destroys previous children under `EncounterContainer`.
- Instantiates the new `EncounterData` runtime clone.
- Updates `IndicatorSprite`.
- Initializes the new encounter visual or enemy preview.
- Leaves authored/random classification under `WorldManager`; replacement never decides whether a tile is eligible.

This prevents duplicate chests, campfires, goblins, or encounter ScriptableObject clones during mid-scene rerolls.

## Enemy scaling

### `EnemyTraversalScaler.cs`

Add a focused Rogue's Path-owned runtime scaler. It receives a newly instantiated enemy, the completed traversal count, and `WorldProgressionSettings`.

Multipliers are:

```text
Health multiplier = 1 + CompletedWorldTraversals × EnemyHealthPerTraversal
Stat multiplier   = 1 + CompletedWorldTraversals × EnemyStatPerTraversal
```

The scaler iterates the runtime enemy's `Stats` dictionary:

- The stat whose ID equals `enemy.MaxHealthID` uses the health multiplier.
- Every other stat uses the general stat multiplier.
- Only each runtime `CharacterStat.BaseValue` is changed.
- After scaling, `enemy.CurrentHealth` is set to the resulting maximum health.

### `CombatManager.cs`

Call the scaler immediately after instantiating `Enemy` and before combat begins. Because every Combat scene creates a fresh enemy clone, scaling is applied exactly once and never mutates `PawnData`, enemy prefabs, equipment, or player stats.

## Traversal announcement

### `WorldManager.cs`

Reuse the existing `DiceRollAnnouncer` from `World.unity`. On traversal completion, set its three text items to:

```text
Traversal Complete
Traversal {CompletedWorldTraversals}
Enemies grow stronger!
```

Then call `Play()`. No new announcement prefab or UI system is required.

## Failure handling

Encounter generation is transactional. The active layout and `WorldEncounterOrder` are changed only after a complete valid bag has been built.

Generation fails with a clear error when:

- `WorldProgressionSettings` cannot be loaded.
- There are too few random tiles for the exact Rest and Treasure requirements.
- `EncounterDatabase` has no valid weighted entry for a required encounter subtype.
- A saved encounter ID is invalid and replacement generation also fails.

A failed traversal generation retains the previous valid layout and does not increment `CompletedWorldTraversals`. If initial generation cannot produce any valid layout, `WorldManager` disables movement rather than allowing the pawn to reach uninitialized tiles.

Missing or undersized `DiceRollAnnouncer.textItems` logs an error but does not prevent traversal completion.

## Verification

Unity playtesting will verify:

- A fresh run starts at traversal zero.
- Initial generation produces exactly two random Rest encounters, one random Treasure encounter, and Combat encounters in all remaining random positions.
- Authored tiles remain unchanged and do not count toward generated quantities.
- Crossing final tile to `StartingTile` increments the counter exactly once.
- Crossing mid-roll rerolls immediately and movement continues.
- Random encounters change while authored encounters remain fixed.
- Generated layout and traversal count survive World, Combat, and Rewards scene transitions.
- A new character resets the counter and generates a new board.
- At traversal 1, enemy maximum health is 120% and other base stats are 110%.
- At traversal 5, enemy maximum health is 200% and other base stats are 150%.
- Enemy current health begins at the scaled maximum.
- Re-entering Combat does not double-scale a runtime enemy.
- Invalid encounter configuration preserves the last valid layout and reports the reason.
- The existing DiceRollAnnouncer displays the traversal message.

## Deferred work

Loot quantity and quality scaling are intentionally excluded. The persisted `CompletedWorldTraversals` value and centralized `WorldProgressionSettings` asset provide the input for that subsequent feature.
