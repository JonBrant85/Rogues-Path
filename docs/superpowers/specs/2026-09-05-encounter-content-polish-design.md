# Encounter Content Polish Design

## Goal

Finish the existing Rest and Treasure encounters without expanding their mechanics. Both encounters must clearly present their outcome, require the player to acknowledge it, and leave a resolved world visual behind.

## Scope

This pass keeps the established encounter behaviors:

- Rest restores 30% of the player's maximum health.
- Treasure offers three unique equipment choices using its existing quality weights and grants one selection.
- Rest uses the existing campfire prefab.
- Treasure uses the existing FantasyMonsters treasure-chest prefab.

It does not add alternate Rest actions, additional reward types, new encounter variants, new scenes, or new UI controllers. It does not edit third-party assets or either world-visual prefab.

## Shared Encounter Lifecycle

`EncounterData` retains the `Transform` returned when its runtime clone instantiates a world visual. This reference belongs to the runtime clone only and gives Rest and Treasure access to the specific visual created for their tile.

`UIEncounterWindow` adds a reusable result stage. It clears temporary equipment-choice controls, replaces the body copy with an outcome message, creates a Continue button from the existing button prefab, and waits for the click. The encounter window stays open and the World bottom bar stays hidden throughout the complete lifecycle:

1. Show encounter title and description.
2. Wait for the encounter action or equipment selection.
3. Apply the encounter effect.
4. Update the world visual.
5. Show the result message.
6. Wait for Continue.
7. Hide the encounter window and restore World controls.

The existing confirmation-button creation and cleanup logic remains the single button path used by Rest, traversal completion, and the new result stage. Result presentation must not leave equipment slots, outlines, buttons, or listeners behind.

## Rest Encounter

### Player-facing copy

- Title: `A Quiet Flame`
- Description: `A lonely fire burns beside the path, offering a moment of warmth and shelter.`
- Damaged result: `The fire's warmth settles into your bones. You recover {amount} health.`
- Full-health result: `You rest beside the flames, but your strength is already whole.`
- Result button: `Continue`

The result strings remain serialized on `RestEncounter`, and `{amount}` is replaced with the actual restored-health value formatted without unnecessary decimal places.

### Behavior

Clicking Rest always consumes the campfire. The encounter calls the existing `WorldManager.HealPlayer` path. When the returned amount is greater than zero, it displays the damaged result. When it is zero because the player is already at maximum health, it displays the full-health result.

After the action, Rest finds every child `ParticleSystem` beneath its retained runtime visual, stops emission, and clears live particles. It also disables child `Light` components. The logs remain visible as a spent campsite. Extinguishing occurs for both damaged and full-health players.

If the runtime visual or its fire components are missing, Rest logs a warning but still applies healing, shows the correct result, and permits Continue. Visual polish must never block encounter completion.

## Treasure Encounter

### Player-facing copy

- Title: `Abandoned Cache`
- Description: `A weathered chest lies half-hidden beside the road. Its lock has long since surrendered.`
- Success result: `Inside, you find {item} ({quality}). You secure it among your belongings.`
- Empty result: `The chest opens with a hollow creak. Whatever it once held is long gone.`
- Result button: `Continue`

The result strings remain serialized on `TreasureEncounter`. `{item}` and `{quality}` are replaced with the selected equipment's display name and quality.

### Selection safety

`EquipmentDatabase.Instance.Equipment` already returns a defensive list copy. Treasure continues to filter, shuffle, and truncate that returned copy. It must not bypass the property or gain access to the database's private serialized list; the shared database contents and order remain unchanged after any number of Treasure encounters.

The encounter continues to create per-instance `EquipmentInstanceData`, preserve its current weighted-quality selection, add the chosen instance to `Game.Instance.PlayerInventory`, and raise `InventoryChanged` once.

### Resolved visual and result

After a valid selection is granted, Treasure finds the existing FantasyMonsters `Monster` component on its retained runtime visual and calls its built-in death animation as the chest's opening/break effect. Empty and invalid-selection result paths also open the chest before revealing that it contains nothing. The opened chest remains visible through the result stage and afterward.

If the animation component is missing, Treasure logs a warning but still grants the item and shows the success result. If the database contains no usable equipment or the UI cannot produce a valid selection, the encounter logs the configuration error, shows the empty result, and requires Continue instead of closing abruptly.

## Error Handling

- A missing result-body reference or confirmation-button dependency logs an error and exits without waiting forever.
- Destroyed encounter-window objects terminate pending UI waits safely.
- Missing visual-effect components degrade to warnings because gameplay effects remain valid.
- Invalid Treasure data never modifies the inventory and never mutates the equipment database.
- Every temporary button and equipment-choice object removes its listeners and is destroyed before the encounter closes.

## Files

- `Assets/_Rogues Path/World/Encounters/EncounterData.cs`
- `Assets/_Rogues Path/World/Encounters/UIEncounterWindow.cs`
- `Assets/_Rogues Path/World/Encounters/RestEncounter.cs`
- `Assets/_Rogues Path/World/Encounters/TreasureEncounter.cs`
- `Assets/_Rogues Path/World/Encounters/ScriptableObjects/Rest.asset`
- `Assets/_Rogues Path/World/Encounters/ScriptableObjects/Treasure.asset`

No scene, prefab, third-party, database asset, or project-settings edits are required.

## Verification

Static verification will confirm:

- Treasure continues to operate on the defensive list returned by `EquipmentDatabase.Equipment`.
- Result copy and Continue settings are serialized in both encounter assets.
- Runtime world visuals are retained per instantiated encounter.
- Rest stops particle systems and disables lights.
- Treasure invokes the existing chest animation without modifying third-party code.
- Only the six scoped files and this specification change.

Unity Editor verification remains the runtime gate:

1. Enter Rest while damaged, verify exactly 30% maximum health is restored up to the normal cap, the fire extinguishes, the result includes the actual amount, and World controls return only after Continue.
2. Enter Rest at full health, verify no health changes, the full-health result appears, the fire still extinguishes, and Continue completes the encounter.
3. Enter Treasure, verify three unique choices appear, Select enables only after choosing an item, the selected instance enters inventory once, the chest opens, and the result includes its name and quality.
4. Resolve multiple Treasure encounters and verify the equipment database count, contents, and order remain unchanged.
5. Exercise missing/invalid visual and equipment configuration paths and verify they log clearly without hanging the encounter window.
