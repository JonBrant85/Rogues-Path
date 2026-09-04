# Game Over Screen Design

**Date:** 2026-09-04
**Branch:** `codex/game-over-screen`

## Goal

Add a complete defeat path to Rogue's Path. When the player dies in combat, the game must finish the killing command, leave the death animation visible for 1.5 seconds, transition to a dedicated Game Over state and scene, and present Main Menu and Quit actions.

The first version deliberately excludes run statistics. Its scene and controller must remain simple to extend with a run-summary panel later.

## Current failure

`CommandInvoker` fires `Trigger.GameOver` after detecting a dead player. The state machine is still in `State.Combat`, but `State.Combat` does not permit that trigger and there is no `State.GameOver` configuration. Stateless therefore rejects the trigger instead of ending the encounter.

`State.Combat.OnExit` also currently prepares rewards and loads the Rewards scene for every exit. Adding a defeat transition without separating that behavior would grant rewards on death and race the Game Over scene load.

## Selected approach

Use a dedicated state, scene, and screen controller.

- `State.GameOver` owns the defeat screen lifecycle.
- `GameOver.unity` is a standalone scene loaded through the existing loading-screen system.
- `UIGameOver` owns the Main Menu and Quit button listeners.
- Victory preparation moves into `State.RewardsScreen.OnEntry` so reward work occurs only after the victory trigger.
- `CommandInvoker` resolves combat outcomes in a mutually exclusive order, with player death taking precedence.

This follows the current scene-per-state architecture and keeps defeat UI out of the Combat scene. A component-only scene driven entirely by `GameStateTriggerer` was rejected because screen-specific actions would be scattered across serialized objects. A Combat overlay was rejected because it would couple defeat presentation and input blocking to Combat.

## State machine

Add `GameOver` to the `State` enum. The existing `Trigger.GameOver` remains the defeat trigger.

`State.Combat` must permit:

- `Trigger.EnterRewardsScreen` to `State.RewardsScreen`.
- `Trigger.GameOver` to `State.GameOver`.
- Its existing supported transitions that remain valid outside combat resolution.

Remove reward generation and Rewards scene loading from generic `State.Combat.OnExit`.

`State.RewardsScreen.OnEntry` will:

1. Generate the two pending equipment rewards.
2. Generate or increment the pending orb reward.
3. Load the configured Rewards scene.
4. Raise the existing inventory-change event as it does today.

`State.GameOver.OnEntry` will load the configured Game Over scene. `State.GameOver` permits `Trigger.EnterMainMenu` to `State.MainMenu`.

Returning to Main Menu uses the normal state transition. Main Menu continues to load through its existing `OnEntry` behavior.

## Combat outcome resolution

After the command queue has drained, `CommandInvoker` evaluates the combatants once.

Outcome order is intentional:

1. If the player is dead, wait 1.5 seconds, fire `Trigger.GameOver`, raise one `CombatEncounterEnded` event, and stop processing.
2. Otherwise, if the enemy is dead, save the player's health, wait 1.5 seconds, fire `Trigger.EnterRewardsScreen`, and raise one `CombatEncounterEnded` event.
3. Otherwise, do nothing.

Player death takes precedence if both combatants are dead. The two outcomes must use mutually exclusive control flow so a simultaneous death never fires both triggers or loads both scenes.

The delay begins only after the killing command completes. Existing brain guards already stop dead combatants from choosing another action during the pause.

## Persistent game and scene wiring

`Game.cs` gains a serialized `SceneField` named `GameOverScene` under the existing Scenes foldout.

The persistent `Game` component in `_LoadingScreen.unity` must reference the new `GameOver.unity` asset. `GameOver.unity` must also be enabled in `ProjectSettings/EditorBuildSettings.asset`.

The Game Over scene depends on the persistent `Game` instance created by the normal boot flow. Directly opening the scene is not a supported gameplay entry point.

Returning to Main Menu does not introduce a second run-reset system. The existing character-selection lock-in remains the authority for initializing a new run. Defeat does not generate, grant, or clear pending rewards.

## Game Over scene

Create `Assets/_Rogues Path/Scenes/GameOver.unity` with:

- A screen-space Canvas using the Main Menu reference resolution and scaling behavior.
- An EventSystem.
- Existing Main Menu background, font, and button styling wherever practical.
- A centered `GAME OVER` heading.
- The subtitle `Your journey has ended.`
- A vertical Main Menu button followed by a Quit button.
- One `UIGameOver` component with serialized references to both buttons.

The initial screen contains no retry action, reward display, statistics, or run-summary data.

The layout leaves room between the subtitle and buttons for a future run-summary panel. No unused statistics model, placeholder serialized fields, or speculative tracking system will be added in this version.

## Screen controller

Add `UIGameOver` under `Assets/_Rogues Path/UI/GameOver/`.

The controller will:

- Register both button listeners during initialization.
- Disable the Main Menu button when selected to prevent duplicate state triggers.
- Fire `Trigger.EnterMainMenu` for Main Menu.
- Call `Application.Quit()` for Quit.
- Remove listeners when destroyed.

The controller contains no combat outcome logic, reward logic, save logic, or run-statistics logic.

## Failure handling

- A defeat must emit exactly one state trigger and one `CombatEncounterEnded` event.
- Victory must continue to emit exactly one victory trigger and event.
- Game Over must not prepare rewards or load the Rewards scene.
- The Main Menu button must not be able to fire the transition twice.
- Scene references are serialized and verified before publication; a missing Game Over scene assignment is a failed implementation gate.
- Quit has no visible effect in the Unity Editor by design; its player-build behavior is the acceptance target.

## Files in scope

- `Assets/_Rogues Path/_Game/Scripts/Game.cs`
- `Assets/_Rogues Path/_Game/Scripts/GameState.cs`
- `Assets/_Rogues Path/PawnCommands/Scripts/CommandInvoker.cs`
- `Assets/_Rogues Path/UI/GameOver/UIGameOver.cs`
- `Assets/_Rogues Path/UI/GameOver/UIGameOver.cs.meta`
- `Assets/_Rogues Path/UI/GameOver.meta`
- `Assets/_Rogues Path/Scenes/GameOver.unity`
- `Assets/_Rogues Path/Scenes/GameOver.unity.meta`
- `Assets/_Rogues Path/Scenes/_LoadingScreen.unity`
- `ProjectSettings/EditorBuildSettings.asset`

Existing reusable UI assets may be referenced but must not be modified unless implementation reveals a necessary, separately approved change.

## Verification

The repository has Unity Test Framework installed but no project test assembly, and the project has intentionally not added one. Verification therefore has two layers.

### Static checks

- A before/after source contract proves the current branch lacks the Game Over state transition and then confirms the completed wiring.
- `State.GameOver`, the Combat permit, Game Over entry, and Main Menu permit all exist.
- Generic Combat exit no longer generates rewards or loads the Rewards scene.
- Rewards are generated and the Rewards scene is loaded only from the RewardsScreen path.
- `CommandInvoker` waits 1.5 seconds and selects exactly one outcome, with player death first.
- `UIGameOver` owns both actions and prevents duplicate Main Menu transitions.
- The new scene and script metadata GUIDs resolve from every serialized reference.
- `_LoadingScreen.unity` assigns `GameOverScene` to `GameOver.unity`.
- `GameOver.unity` is enabled in Editor build settings.
- The committed diff contains no unrelated gameplay, prefab, or third-party changes.

### Unity acceptance checks

1. Allow Unity to import and compile with no errors.
2. Win combat and confirm the existing 1.5-second pause, reward preparation, Rewards scene, and reward collection still work.
3. Lose combat and confirm the player death animation remains visible for 1.5 seconds.
4. Confirm no Stateless unhandled-trigger error appears.
5. Confirm only the Game Over scene loads and no rewards are generated.
6. Confirm the Game Over heading, subtitle, Main Menu button, and Quit button render at the expected resolution.
7. Select Main Menu and confirm the game returns through the normal state transition without duplicate-trigger errors.
8. Verify Quit in a player build when convenient; no-op behavior in the Editor is expected.
9. Exercise a simultaneous-death case if one is available and confirm defeat wins without a Rewards transition.

## Future extension: run summaries

A later feature may insert a run-summary panel between the subtitle and buttons. That work should define which statistics are authoritative, when they reset, and where they are accumulated before adding UI fields.

The initial Game Over implementation must not block that layout extension, but it must not collect speculative data or add an unused summary model.

## Scope exclusions

- Run-statistics collection or display.
- Retry, resurrection, checkpoint rollback, or combat replay.
- Changes to character-selection run initialization.
- New save/load behavior.
- Game Over animation, audio, achievements, or online reporting.
- Refactoring unrelated game states or UI screens.
