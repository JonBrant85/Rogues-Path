# Goblin and Skeleton Prefab Hierarchy Design

**Date:** 2026-09-04  
**Branch:** `Encounters`

## Goal

Make `Goblin.prefab` the canonical enemy-pawn base and keep `Skeleton Variant.prefab` as a clean visual variant. Gameplay components remain on the pawn root, character rendering and animation components live together on one model child, and each variant overrides only the model-specific Pawn references.

## Current problem

`Goblin.prefab` currently places `Character4D`, `AnimationManager`, `AnimationEvents`, `LayerManager`, `Animator`, `SortingGroup`, and Animazing on the Pawn root while its existing `Model` child contains only the directional visual hierarchy.

`Skeleton Variant.prefab` removes the Goblin model, adds a separate Skeleton model with its own visual and animation components, and retains several inherited components on the root. This produces duplicate or orphaned animation drivers and previously caused `Pawn.animationManager.Jab()` to target the abandoned Goblin Animator instead of the rendered Skeleton.

## Considered approaches

### 1. Canonical model-child boundary — selected

Move all rendering and animation components into the existing Goblin `Model` child. Keep gameplay, hover detection, and the status-display child on the Pawn root. Make the Skeleton variant replace only the model child and override its three model references.

This creates one predictable boundary for every future enemy variant and removes the inherited-component ambiguity that caused the Skeleton Jab failure.

### 2. Preserve the current base and maintain variant overrides

Leave the Goblin components on the root and explicitly override every affected reference in each variant. This requires less immediate prefab surgery but leaves every future enemy vulnerable to the same missing-reference and duplicate-Animator mistakes.

### 3. Make Goblin and Skeleton independent prefabs

Break the prefab-variant relationship and maintain two complete Pawn prefabs. This removes inheritance complexity but duplicates stats, brain, status display, collider, and Pawn configuration. The duplicated gameplay setup would drift over time.

## Approved hierarchy

```text
Pawn Root
├── UIStatusDisplay
└── Model
    ├── Front
    ├── Back
    ├── Left
    ├── Right
    └── Shadow
```

For `Skeleton Variant.prefab`, the variant model child may retain the descriptive name `Skeleton`; it occupies the same structural position and owns the same component categories as the Goblin `Model` child.

## Component ownership

### Pawn root

The root owns only shared gameplay and interaction behavior:

- `Pawn`
- `PawnBrain`
- one `CapsuleCollider2D`

The existing `UIStatusDisplay` remains a separate child of the root. Showing or hiding the status display must never disable the Pawn root or model.

### Model child

Each model child owns its complete character presentation stack:

- `Character4D`
- `AnimationManager`
- `AnimationEvents`
- `LayerManager`
- `Animator`
- `SortingGroup`
- `Animazing`
- the directional visual hierarchies and shadow

Animazing must share a GameObject with `Animator`. Its runtime `AnimationLayer` helpers use `GetComponent<Animator>()`, so leaving Animazing on the Pawn root would break commands that use `Pawn.PlayAnimation()` after the Animator moves into the model.

The collider deliberately remains on the Pawn root. `Pawn.OnMouseEnter()` and `Pawn.OnMouseExit()` also live there and control the status display.

## Goblin base migration

### `Goblin.prefab`

Use the existing `Model` child as the presentation boundary.

Move the seven presentation components from the Goblin root to `Model` without recreating them. Preserving their existing component file IDs keeps serialized references stable.

Update ownership lists and references so:

- `Pawn.Character` points to the `Character4D` on `Model`.
- `Pawn.animationManager` points to the `AnimationManager` on `Model`.
- `Pawn.animazing` points to Animazing on `Model`.
- `Character4D.Animator`, `Character4D.AnimationManager`, and `Character4D.LayerManager` point to components on `Model`.
- `AnimationManager.Character` and `AnimationManager.Animator` point to components on `Model`.
- `LayerManager.SortingGroup` points to the `SortingGroup` on `Model`.
- The existing directional objects remain children of `Model`.
- The existing root collider and `UIStatusDisplay` child remain unchanged.

No scripts, animation clips, Animator controllers, sprites, or vendor assets change.

## Skeleton variant migration

### `Skeleton Variant.prefab`

Keep the prefab based on `Goblin.prefab`.

After the Goblin presentation stack moves into its `Model` child, removing that inherited child removes the entire Goblin presentation stack together. Clean obsolete overrides that previously attempted to remove, clear, or redirect individual inherited visual components.

The Skeleton model child retains its own:

- `Character4D`
- `AnimationManager`
- `AnimationEvents`
- `LayerManager`
- `Animator`
- `SortingGroup`
- directional visual hierarchies and shadow

Add Animazing to the Skeleton model child so it shares the Skeleton Animator's GameObject. Remove the duplicate `CapsuleCollider2D` from the Skeleton model; the variant inherits the single collider on the Pawn root.

The variant explicitly overrides:

- `Pawn.Character` to the Skeleton `Character4D`.
- `Pawn.animationManager` to the Skeleton `AnimationManager`.
- `Pawn.animazing` to the Skeleton Animazing component.

The root identity overrides (`CharacterName` and `ClassName`) remain intact. The `Skeleton.asset` PawnData continues referencing the Skeleton variant and Jab.

## Inheritance contract for future enemies

A future enemy variant replaces the inherited Goblin model with its own model child, gives that child the complete presentation stack, and overrides only `Pawn.Character`, `Pawn.animationManager`, and `Pawn.animazing`.

Shared gameplay behavior, brain configuration, collider, and status-display hierarchy continue to come from `Goblin.prefab` unless an enemy deliberately overrides them.

## Verification

Static validation will check:

- Goblin root contains Pawn, PawnBrain, and exactly one collider, with no Animator, Character4D, AnimationManager, Animazing, LayerManager, AnimationEvents, or SortingGroup.
- Goblin `Model` contains the complete presentation stack and all directional children.
- Skeleton root inherits the shared gameplay components and root collider.
- Skeleton model contains the complete presentation stack but no collider.
- Each Pawn's Character, AnimationManager, and Animazing references target its active model.
- The Skeleton remains a variant of `Goblin.prefab`.
- `UIStatusDisplay` remains a distinct child of the Pawn root.
- Only `Goblin.prefab` and `Skeleton Variant.prefab` change during implementation.

Unity playtesting will verify:

- Goblin and Skeleton instantiate in Combat with the correct appearance and facing.
- Jab visibly animates both enemies.
- A command using `Pawn.PlayAnimation()` animates the rendered model rather than an orphaned Animator.
- Hovering each enemy shows its status display, and leaving hover hides it.
- Showing or hiding the status display never disables the Pawn or model.
- Taking damage and dying continue using the rendered model.
- No duplicate colliders or duplicate Animator-driven motion remain.

Unity compilation and prefab import remain runtime gates because the current Codex environment does not include the Unity Editor.

## Scope exclusions

- No animation-command architecture changes.
- No Animator Controller or animation-clip edits.
- No enemy-stat, spell, encounter, or visual-art changes.
- No changes to third-party HeroEditor4D or Animazing source.
- No conversion of existing player prefabs; this design establishes the enemy-prefab pattern only.
