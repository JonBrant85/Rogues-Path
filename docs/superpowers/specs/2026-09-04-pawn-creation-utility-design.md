# Pawn Creation Utility Design

**Date:** 2026-09-04  
**Branch:** `Encounters`

## Goal

Add a `Pawn Creation` tab to `UtilityWindow` that converts a raw HeroEditor4D model prefab into a Pawn prefab variant in place. The first acceptance target is `Skeleton Archer.prefab`, using `Goblin.prefab` as its default base.

The generated variant must follow the enemy hierarchy already proven by the Goblin and Skeleton Jab fixes: shared gameplay and hover behavior come from the Pawn root, while all rendering and animation behavior stays together on the replacement model child.

## Selected approach

Use Unity's prefab APIs to construct and save a real prefab variant. Do not edit prefab YAML directly.

Unity 2022.3 does not support removing a GameObject inherited from a prefab-variant base. The tool therefore renames the inherited base model to `Base Model (Inactive)` and deactivates it, then adds the raw model as the sole active `Model` child.

The tool will preserve the target prefab's asset path and `.meta` file, so references to the raw prefab continue resolving to the converted variant. Before overwriting the prefab contents, the tool will show an explicit confirmation dialog.

Alternative approaches were rejected:

- Creating a second output prefab would preserve the raw source but leave duplicate assets and violate the requested in-place workflow.
- Editing YAML directly would be fragile across Unity serialization changes and could create invalid inheritance metadata.

## Editor interface

`UtilityWindow.cs` gains a third toolbar tab named `Pawn Creation`.

All Pawn Creation fields, validation helpers, conversion logic, cleanup, and result validation live in `UtilityWindow.cs`. The feature does not introduce a separate service or helper file.

The tab contains:

- `Model Prefab`: the raw prefab asset to convert.
- `Base Pawn Prefab`: the Pawn prefab from which the result will inherit.
- `Create Pawn Variant`: the conversion button.

`Base Pawn Prefab` defaults to:

`Assets/_Rogues Path/Pawns/Prefabs/EnemyPawns/Goblin.prefab`

The base remains selectable so the utility can support other canonical Pawn bases later without changing code.

## Input contract

The conversion runs only when both object fields reference persistent prefab assets.

The raw model root must contain exactly the model-side components needed by Rogue's Path:

- `Character4D`
- `AnimationManager`
- `AnimationEvents`
- `LayerManager`
- `Animator`
- `SortingGroup`

The raw model may contain one or more `Collider2D` components; all model-side colliders are removed during conversion because interaction belongs to the Pawn root. If Animazing is already present beside the Animator, it is reused; otherwise it is added.

The selected base must contain:

- `Pawn` on its root.
- A `PawnBrain` assigned to `Pawn.Brain`.
- Exactly one enabled `Collider2D` on its root.
- A direct child containing the assigned `UIStatusDisplay`.
- A direct model child referenced by `Pawn.Character`, `Pawn.animationManager`, and `Pawn.animazing`.

The tool also requires a sorting layer named `Pawns`.

The target is rejected before modification when it already contains a `Pawn`, is already a prefab variant, is the selected base asset, lacks any required model component, or would otherwise create a prefab dependency cycle.

## Conversion behavior

After validation and user confirmation, the utility performs one conversion:

1. Preserve a disconnected copy of the raw model root and its complete child hierarchy.
2. Instantiate the selected base Pawn as the new prefab root while retaining its prefab connection, then name that root after the target prefab file.
3. Rename the inherited base model to `Base Model (Inactive)` and deactivate it.
4. Rename the preserved raw model root to `Model`, reset its local position and rotation, retain its local scale, and attach it beneath the Pawn root.
5. Remove every `Collider2D` from the new `Model` object.
6. Add or reuse Animazing on `Model`, beside its `Animator`.
7. Set the model `SortingGroup.sortingLayerName` to `Pawns`.
8. Normalize the replacement model's internal references:
   - `Character4D.Animator`, `Character4D.AnimationManager`, and `Character4D.LayerManager` target components on `Model`.
   - `AnimationManager.Character` and `AnimationManager.Animator` target components on `Model`.
   - `LayerManager.SortingGroup` targets the model's `SortingGroup`.
9. Override the inherited Pawn's model references:
   - `Pawn.Character` targets the model's `Character4D`.
   - `Pawn.animationManager` targets the model's `AnimationManager`.
   - the serialized `Pawn.animazing` field targets the model's Animazing component.
10. Save the connected base instance over the original raw model prefab path as a prefab variant.
11. Reimport the saved asset, select it in the Project window, and log a concise success message.

The converted prefab retains the raw model's child objects, sprites, Avatar/controller references, component settings, and model scale. Root gameplay values, the brain, root collider, and `UIStatusDisplay` are inherited from the selected base.

The utility does not create or modify `PawnData`; identity, stats, equipment, and prepared spells remain separate authoring work.

## Resulting hierarchy

```text
Pawn Variant Root
├── UIStatusDisplay          (inherited)
├── Base Model (Inactive)    (inherited and disabled)
└── Model                    (replacement)
    ├── Front
    ├── Back
    ├── Left
    ├── Right
    └── Shadow
```

### Root responsibilities

- `Pawn`
- assigned `PawnBrain`
- one root `Collider2D`
- independent `UIStatusDisplay` child
- one disabled inherited base model

### Model responsibilities

- `Character4D`
- `AnimationManager`
- `AnimationEvents`
- `LayerManager`
- `Animator`
- `SortingGroup` on the `Pawns` sorting layer
- Animazing
- directional presentation hierarchy

Animazing must remain on the same GameObject as the Animator because its animation-layer helpers resolve the Animator locally.

## Failure behavior

Validation is completed before the target asset is changed. Validation failure displays a specific dialog and logs the same reason.

If conversion throws after modification begins, the utility must not leave temporary scene objects behind. It logs the exception and prompts the user to restore the target through source control if Unity partially wrote the asset. The operation will not claim Undo support for the asset overwrite because Unity does not reliably make prefab-file replacement undoable.

The confirmation dialog identifies the exact target path and base prefab and states that the target prefab will be replaced in place.

## Verification

Static checks will verify that `UtilityWindow.cs` contains:

- The new tab and both prefab object fields.
- Validation before the confirmation and save operations.
- Prefab-variant creation through Unity prefab APIs rather than YAML manipulation.
- Model collider removal.
- Animazing and Animator co-location enforcement.
- `Pawns` sorting-layer assignment.
- All six internal model-reference assignments.
- All three Pawn model-reference assignments.
- Cleanup in a `finally` path for temporary objects or prefab contents.
- The complete feature implementation; no additional Pawn Creation source file is introduced.

Because this repository has Unity Test Framework installed but no test assembly, and the project previously declined adding test infrastructure, Unity Editor behavior remains a manual gate.

The Skeleton Archer acceptance test will verify:

- `Skeleton Archer.prefab` remains at the same path and GUID.
- It is now a variant of `Goblin.prefab`.
- Its model art and directional hierarchy are unchanged.
- It inherits Goblin's root Pawn, brain, collider, and status display.
- The inherited Goblin model remains disabled as `Base Model (Inactive)` and only the Archer model renders.
- The Archer model has the complete presentation stack and no collider.
- Its Sorting Group uses the `Pawns` layer.
- Its Character4D, AnimationManager, and LayerManager reference the Archer model's own components.
- `Pawn.Character`, `Pawn.animationManager`, and `Pawn.animazing` reference the Archer model.
- The prefab imports without missing-script or missing-reference warnings.
- It can be instantiated and animated in Play Mode.
- Hover shows and hides only the status display, never the Pawn root.

## Scope exclusions

- No `PawnData` creation.
- No automatic enemy naming, stats, equipment, spells, or encounter registration.
- No Animator Controller or animation-clip changes.
- No batch conversion.
- No conversion of already-derived variants.
- No third-party HeroEditor4D or Animazing source changes.
- No changes to the existing Goblin or Skeleton prefabs.
- No new Pawn Creation service or helper source file.
