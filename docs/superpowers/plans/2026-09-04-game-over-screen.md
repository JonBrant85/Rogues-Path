# Game Over Screen Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the unhandled combat-death trigger with a complete Game Over state and scene containing Main Menu and Quit actions.

**Architecture:** `CommandInvoker` resolves defeat and victory as mutually exclusive outcomes after a 1.5-second presentation delay. The state machine moves victory-only reward preparation into `RewardsScreen.OnEntry`, adds a dedicated `GameOver` state, and loads a standalone uGUI scene whose small controller owns only its two buttons.

**Tech Stack:** Unity 2022.3.62f2, C#, Stateless, UniTask, Unity uGUI, serialized Unity scene YAML.

**Spec:** `docs/superpowers/specs/2026-09-04-game-over-screen-design.md`

## Global Constraints

- Work only on `codex/game-over-screen`, based on the latest `origin/Encounters` used to create the worktree.
- The defeat delay is exactly 1.5 seconds and starts after the killing command completes.
- Player death wins a simultaneous-death outcome; exactly one state trigger and one `CombatEncounterEnded` event may fire.
- Defeat must not prepare rewards or load the Rewards scene.
- The screen contains Main Menu and Quit only; no retry flow or run-summary data is added.
- Reuse the Main Menu's background sprite, 3840x2160 Canvas scaling, RPG UI button prefab, and font assets.
- Add `State.GameOver` at the end of the enum so existing serialized enum values do not shift.
- Do not add a Unity test assembly. Use failing/passing source and serialized-asset contracts, followed by the user's Unity compile and playtest.
- Do not modify third-party assets.

---

### Task 1: Add mutually exclusive combat outcomes and Game Over state flow

**Files:**

- Modify: `Assets/_Rogues Path/PawnCommands/Scripts/CommandInvoker.cs`
- Modify: `Assets/_Rogues Path/_Game/Scripts/Game.cs`
- Modify: `Assets/_Rogues Path/_Game/Scripts/GameState.cs`

**Interfaces:**

- Consumes: `CombatManager.Instance.Player.IsDead`, `CombatManager.Instance.Enemy.IsDead`, `PlayerHealthState.Save(Pawn)`, `Game.FireTrigger(Trigger)`, and the existing pending-reward collections.
- Produces: `State.GameOver`, serialized `Game.GameOverScene`, `Combat -> GameOver`, `GameOver -> MainMenu`, and victory-only reward preparation.

- [ ] **Step 1: Run the failing state-flow source contract**

Run:

```bash
set -euo pipefail
state="Assets/_Rogues Path/_Game/Scripts/GameState.cs"
game="Assets/_Rogues Path/_Game/Scripts/Game.cs"
invoker="Assets/_Rogues Path/PawnCommands/Scripts/CommandInvoker.cs"

state_enum="$(sed -n '/public enum State {/,/^    }/p' "$state")"
grep -q '^        GameOver$' <<<"$state_enum"
rg -q '\.Permit\(Trigger\.GameOver, State\.GameOver\)' "$state"
rg -q 'gameState\.Configure\(State\.GameOver\)' "$state"
rg -q 'GameOverScene' "$game"
rg -U -q '(?s)if \(allPlayersDead\).*UniTask\.Delay\(1500\).*Trigger\.GameOver.*return;.*if \(allEnemiesDead\)' "$invoker"

combat_block="$(sed -n '/gameState.Configure(State.Combat)/,/gameState.Configure(State.RewardsScreen)/p' "$state")"
rewards_block="$(sed -n '/gameState.Configure(State.RewardsScreen)/,/gameState.Configure(State.WorldMap)/p' "$state")"
! grep -q 'PendingEquipmentRewards\|PendingOrbRewards\|LoadScene(Rewards)' <<<"$combat_block"
grep -q 'PendingEquipmentRewards' <<<"$rewards_block"
grep -q 'PendingOrbRewards' <<<"$rewards_block"
grep -q 'LoadScene(Rewards)' <<<"$rewards_block"
```

Expected: exit `1` at the missing `State.GameOver` assertion. This proves the contract detects the current unhandled-trigger design.

- [ ] **Step 2: Make combat outcome resolution mutually exclusive**

Replace the two independent terminal `if` blocks in `CommandInvoker.ExecuteCommand` with:

```csharp
bool allPlayersDead = CombatManager.Instance.Player.IsDead;
bool allEnemiesDead = CombatManager.Instance.Enemy.IsDead;

if (allPlayersDead) {
    await UniTask.Delay(1500);
    Game.FireTrigger(Trigger.GameOver);
    EventBus.Raise(new CombatEncounterEnded());
    return;
}

if (allEnemiesDead) {
    PlayerHealthState.Save(CombatManager.Instance.Player);
    await UniTask.Delay(1500);
    Game.FireTrigger(Trigger.EnterRewardsScreen);
    EventBus.Raise(new CombatEncounterEnded());
}
```

The early return is required: it makes player death authoritative when both booleans are true.

- [ ] **Step 3: Add the serialized Game Over scene field without shifting existing fields**

In `Game.cs`, append this field beneath the other scene fields:

```csharp
[SerializeField, FoldoutGroup("Scenes")] private SceneField GameOverScene;
```

Do not rename the existing `Rewards` or `World` fields; `_LoadingScreen.unity` already serializes those exact names.

- [ ] **Step 4: Add the state and move reward work to the victory state**

Append `GameOver` to the `State` enum:

```csharp
public enum State {
    Boot,
    InitialLoad,
    MainMenu,
    CharacterSelection,
    LevelSelection,
    WorldMap,
    Combat,
    RewardsScreen,
    GameOver
}
```

Change the Combat configuration so it loads Combat on entry, has no generic reward-generating exit callback, and permits Game Over:

```csharp
gameState.Configure(State.Combat)
    .OnEntry(
        () => {
            LoadingScreenManager.Instance.LoadScene(Combat);
        })
    .Permit(Trigger.EnterMainMenu, State.MainMenu)
    .Permit(Trigger.EnterRewardsScreen, State.RewardsScreen)
    .Permit(Trigger.GameOver, State.GameOver);
```

Move the existing equipment/orb reward preparation and Rewards scene load into the beginning of `State.RewardsScreen.OnEntry`. Preserve the existing `InventoryChanged` event:

```csharp
gameState.Configure(State.RewardsScreen)
    .OnEntry(
        () => {
            for (int i = 0; i < 2; i++) {
                if (EquipmentDatabase.GetIDByName(
                        EquipmentDatabase.Instance.Equipment.GetRandomElement().Name,
                        out int ID)) {
                    Instance.PendingEquipmentRewards.Add(ID);
                }
            }

            Orb orb = OrbDatabase.Instance.GetRandomOrb();

            if (Instance.PendingOrbRewards.ContainsKey(orb)) {
                PendingOrbRewards[orb]++;
            }
            else {
                PendingOrbRewards.Add(orb, 1);
            }

            LoadingScreenManager.Instance.LoadScene(Rewards);
            EventBus.Raise(new InventoryChanged());
        })
    .OnExit(
        () => {
            UIRewardsScreen.Hide();
        })
    .Permit(Trigger.EnterWorld, State.WorldMap);
```

Add the Game Over configuration after RewardsScreen:

```csharp
gameState.Configure(State.GameOver)
    .OnEntry(
        () => {
            LoadingScreenManager.Instance.LoadScene(GameOverScene);
        })
    .Permit(Trigger.EnterMainMenu, State.MainMenu);
```

- [ ] **Step 5: Run the same state-flow contract and inspect the diff**

Run the exact command from Step 1.

Expected: exit `0`. Then run:

```bash
git diff --check
git diff --name-only
git diff -- \
  "Assets/_Rogues Path/PawnCommands/Scripts/CommandInvoker.cs" \
  "Assets/_Rogues Path/_Game/Scripts/Game.cs" \
  "Assets/_Rogues Path/_Game/Scripts/GameState.cs"
```

Expected: only the three Task 1 files changed; no reward code remains in generic Combat exit behavior.

- [ ] **Step 6: Commit the state-flow change**

```bash
git add \
  "Assets/_Rogues Path/PawnCommands/Scripts/CommandInvoker.cs" \
  "Assets/_Rogues Path/_Game/Scripts/Game.cs" \
  "Assets/_Rogues Path/_Game/Scripts/GameState.cs"
git commit -m "Add Game Over state flow"
```

---

### Task 2: Add the Game Over screen controller

**Files:**

- Create: `Assets/_Rogues Path/UI/GameOver.meta`
- Create: `Assets/_Rogues Path/UI/GameOver/UIGameOver.cs`
- Create: `Assets/_Rogues Path/UI/GameOver/UIGameOver.cs.meta`

**Interfaces:**

- Consumes: two serialized `UnityEngine.UI.Button` references and `Trigger.EnterMainMenu` from Task 1.
- Produces: `_Rogues_Path.UI.GameOver.UIGameOver`, with scene-callable Main Menu and Quit behavior.

- [ ] **Step 1: Run the failing controller source contract**

Run:

```bash
set -euo pipefail
controller="Assets/_Rogues Path/UI/GameOver/UIGameOver.cs"
test -f "$controller"
rg -q 'class UIGameOver : MonoBehaviour' "$controller"
rg -q 'Button MainMenuButton' "$controller"
rg -q 'Button QuitButton' "$controller"
rg -q 'MainMenuButton\.interactable = false' "$controller"
rg -q 'Game\.FireTrigger\(Trigger\.EnterMainMenu\)' "$controller"
rg -q 'Application\.Quit\(\)' "$controller"
rg -q 'RemoveListener' "$controller"
```

Expected: exit `1` because `UIGameOver.cs` does not exist.

- [ ] **Step 2: Create the controller**

Create the feature directory first:

```bash
mkdir -p "Assets/_Rogues Path/UI/GameOver"
```

Create `UIGameOver.cs` with:

```csharp
using _Rogues_Path._Game;
using UnityEngine;
using UnityEngine.UI;

namespace _Rogues_Path.UI.GameOver {
    public sealed class UIGameOver : MonoBehaviour {
        [SerializeField] private Button MainMenuButton;
        [SerializeField] private Button QuitButton;

        private void Awake() {
            MainMenuButton.onClick.AddListener(ReturnToMainMenu);
            QuitButton.onClick.AddListener(QuitGame);
        }

        private void OnDestroy() {
            MainMenuButton.onClick.RemoveListener(ReturnToMainMenu);
            QuitButton.onClick.RemoveListener(QuitGame);
        }

        private void ReturnToMainMenu() {
            MainMenuButton.interactable = false;
            Game.FireTrigger(Trigger.EnterMainMenu);
        }

        private static void QuitGame() {
            Application.Quit();
        }
    }
}
```

- [ ] **Step 3: Create Unity metadata with the reserved GUIDs**

Create the folder and script `.meta` files using Unity's standard folder and MonoImporter formats. The GUIDs below were generated for this feature and do not occur in the current repository. Use the script GUID exactly in `GameOver.unity` during Task 3.

Folder metadata shape:

```yaml
fileFormatVersion: 2
guid: 54f7fddffe80c5853c87732c21563fff
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData:
  assetBundleName:
  assetBundleVariant:
```

Script metadata shape:

```yaml
fileFormatVersion: 2
guid: 5a0fe220733ad025b05a99b9e39734fa
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData:
  assetBundleName:
  assetBundleVariant:
```

- [ ] **Step 4: Run the same controller contract and validate metadata**

Run the exact command from Step 1, followed by:

```bash
set -euo pipefail
folder_meta="Assets/_Rogues Path/UI/GameOver.meta"
script_meta="Assets/_Rogues Path/UI/GameOver/UIGameOver.cs.meta"
test "$(sed -n 's/^guid: //p' "$folder_meta" | wc -c)" -eq 33
test "$(sed -n 's/^guid: //p' "$script_meta" | wc -c)" -eq 33
! rg -q '<generated-' "$folder_meta" "$script_meta"
git diff --check
```

Expected: both checks exit `0` and both GUIDs are populated.

- [ ] **Step 5: Commit the controller**

```bash
git add \
  "Assets/_Rogues Path/UI/GameOver.meta" \
  "Assets/_Rogues Path/UI/GameOver/UIGameOver.cs" \
  "Assets/_Rogues Path/UI/GameOver/UIGameOver.cs.meta"
git commit -m "Add Game Over screen controller"
```

---

### Task 3: Create and wire the Game Over scene

**Files:**

- Create: `Assets/_Rogues Path/Scenes/GameOver.unity`
- Create: `Assets/_Rogues Path/Scenes/GameOver.unity.meta`
- Modify: `Assets/_Rogues Path/Scenes/_LoadingScreen.unity`
- Modify: `ProjectSettings/EditorBuildSettings.asset`

**Interfaces:**

- Consumes: `Game.GameOverScene` from Task 1, `UIGameOver` and its script GUID from Task 2, Main Menu background sprite GUID `4f370a2b881d16c44bf48659a1b4ff83`, RPG button prefab GUID `24d00d71aee6bed45b2e606cb8ed8793`, and UI font GUID `9e16b9bde4938274b8af7cc304da2fa6`.
- Produces: a build-enabled `GameOver.unity` scene referenced by the persistent `Game` component.

- [ ] **Step 1: Run the failing serialized-scene contract**

Run:

```bash
set -euo pipefail
scene="Assets/_Rogues Path/Scenes/GameOver.unity"
scene_meta="$scene.meta"
loading="Assets/_Rogues Path/Scenes/_LoadingScreen.unity"
build="ProjectSettings/EditorBuildSettings.asset"

test -f "$scene"
test -f "$scene_meta"
scene_guid="$(sed -n 's/^guid: //p' "$scene_meta")"
test "${#scene_guid}" -eq 32
rg -q 'm_Name: Game Over' "$scene"
rg -q 'm_Text: GAME OVER' "$scene"
rg -q 'm_Text: Your journey has ended\.' "$scene"
rg -q 'value: Main Menu' "$scene"
rg -q 'value: Quit' "$scene"
rg -q 'GameOverScene:' "$loading"
rg -q "guid: $scene_guid" "$loading"
rg -q 'path: Assets/_Rogues Path/Scenes/GameOver\.unity' "$build"
rg -q "guid: $scene_guid" "$build"
```

Expected: exit `1` because the scene does not exist.

- [ ] **Step 2: Create the minimal Game Over scene and metadata**

Create `GameOver.unity` as a standalone Unity scene with this exact logical hierarchy:

```text
Game Over
├── Canvas
│   ├── Background
│   ├── Title
│   ├── Subtitle
│   ├── Main Menu
│   └── Quit
└── EventSystem
```

Serialized requirements:

- `Canvas` uses Screen Space Overlay, `CanvasScaler.ScaleWithScreenSize`, reference resolution `3840x2160`, height matching (`m_MatchWidthOrHeight: 1`), and a `GraphicRaycaster`.
- `Background` is a full-stretch non-raycast `Image` using sprite GUID `4f370a2b881d16c44bf48659a1b4ff83`.
- `Title` is centered above the summary extension area, uses legacy uGUI `Text`, font GUID `9e16b9bde4938274b8af7cc304da2fa6`, and text `GAME OVER`.
- `Subtitle` is centered beneath the title with text `Your journey has ended.` using the same font.
- `Main Menu` and `Quit` are instances of button prefab GUID `24d00d71aee6bed45b2e606cb8ed8793`, vertically stacked and labeled exactly `Main Menu` and `Quit`.
- The Canvas owns one `UIGameOver` MonoBehaviour whose `MainMenuButton` and `QuitButton` references point to the two stripped `Button` components from those prefab instances.
- `EventSystem` copies the EventSystem and StandaloneInputModule component configuration from `MainMenu.unity`.
- No Camera is required because the Canvas is Screen Space Overlay.

Create `GameOver.unity.meta` in the standard DefaultImporter scene format with the reserved GUID `c014c75720b169a7bba27792f8402f05`. This GUID does not occur in the current repository and must not be changed independently of the references below.

- [ ] **Step 3: Wire the scene into the persistent Game component**

In the `Game` MonoBehaviour block in `_LoadingScreen.unity`, add:

```yaml
  GameOverScene:
    m_SceneAsset: {fileID: 102900000, guid: c014c75720b169a7bba27792f8402f05, type: 3}
    m_SceneName: GameOver
```

- [ ] **Step 4: Enable the scene in build settings**

Add the scene after Combat in `ProjectSettings/EditorBuildSettings.asset`:

```yaml
  - enabled: 1
    path: Assets/_Rogues Path/Scenes/GameOver.unity
    guid: c014c75720b169a7bba27792f8402f05
```

Use the same actual GUID as the scene metadata and `_LoadingScreen.unity` reference.

- [ ] **Step 5: Run the same serialized-scene contract plus GUID/reference checks**

Run the exact command from Step 1, followed by:

```bash
set -euo pipefail
scene="Assets/_Rogues Path/Scenes/GameOver.unity"
scene_guid="$(sed -n 's/^guid: //p' "$scene.meta")"
script_guid="$(sed -n 's/^guid: //p' "Assets/_Rogues Path/UI/GameOver/UIGameOver.cs.meta")"

test "$(rg -o "guid: $scene_guid" \
  "$scene.meta" \
  "Assets/_Rogues Path/Scenes/_LoadingScreen.unity" \
  "ProjectSettings/EditorBuildSettings.asset" | wc -l)" -eq 3
rg -F -q "m_Script: {fileID: 11500000, guid: $script_guid, type: 3}" "$scene"
test "$(rg -c 'm_SourcePrefab: .*guid: 24d00d71aee6bed45b2e606cb8ed8793' "$scene")" -eq 2
git diff --check
```

Expected: exit `0`; the scene GUID has exactly three serialized appearances, the controller GUID resolves, and the scene has exactly two button prefab instances.

- [ ] **Step 6: Commit the scene wiring**

```bash
git add \
  "Assets/_Rogues Path/Scenes/GameOver.unity" \
  "Assets/_Rogues Path/Scenes/GameOver.unity.meta" \
  "Assets/_Rogues Path/Scenes/_LoadingScreen.unity" \
  "ProjectSettings/EditorBuildSettings.asset"
git commit -m "Add Game Over scene"
```

---

### Task 4: Verify the complete committed feature and run Unity acceptance

**Files:**

- Verify: every file listed in Tasks 1-3.
- Runtime target: player defeat in `Assets/_Rogues Path/Scenes/Combat.unity`.
- Regression target: enemy defeat and reward collection.

**Interfaces:**

- Consumes: all committed Game Over state, controller, scene, and serialized wiring.
- Produces: a reviewed feature branch ready for direct publication after user approval.

- [ ] **Step 1: Run the full committed source and asset contract**

Rerun the exact passing commands from Task 1 Step 1, Task 2 Step 1, and Task 3 Steps 1 and 5 against committed `HEAD`.

Expected: every command exits `0`.

- [ ] **Step 2: Verify scope, metadata, and branch cleanliness**

Run:

```bash
set -euo pipefail
git status --short
git show --check --stat --oneline HEAD
git diff --check origin/Encounters..HEAD
git diff --name-status origin/Encounters..HEAD
git log --oneline origin/Encounters..HEAD
```

Expected:

- The worktree is clean.
- All commits descend from `origin/Encounters`.
- Only the design, plan, and implementation files named by this plan differ.
- No third-party asset, existing UI prefab, combat prefab, or unrelated scene changed.
- Every new Unity asset has exactly one `.meta` file and every serialized GUID resolves.

- [ ] **Step 3: Review the complete state and event flow**

Confirm from the committed diff:

1. `State.GameOver` is appended, not inserted before an existing enum value.
2. Combat has a Game Over permit and no generic reward exit behavior.
3. Rewards are prepared only in RewardsScreen entry.
4. CommandInvoker checks player death first, waits 1500 milliseconds, fires one trigger, raises one event, and returns.
5. Victory still saves player health before its 1500-millisecond delay.
6. The Game Over scene is build-enabled and assigned to the persistent Game object.
7. The controller owns UI actions only.

- [ ] **Step 4: Run the Unity Editor acceptance gate**

In Unity 2022.3.62f2:

1. Pull or open the feature branch and allow all assets to import.
2. Confirm the Console has no compiler, missing-script, broken-prefab, or scene-reference errors.
3. Enter the normal boot flow and win a combat.
4. Confirm the existing 1.5-second victory pause, reward generation, Rewards scene, collection, and Continue flow still work.
5. Enter combat again and allow the player to die.
6. Confirm the death pose remains visible for 1.5 seconds.
7. Confirm `GameOver.unity` loads with the expected background, heading, subtitle, and buttons.
8. Confirm there is no Stateless unhandled-trigger error and no reward is generated on defeat.
9. Click Main Menu once and confirm `MainMenu.unity` loads without a duplicate-trigger error.
10. Verify Quit in a player build when convenient; no visible Editor action is expected.
11. If a simultaneous-death setup exists, confirm Game Over wins and Rewards never loads.

- [ ] **Step 5: Stop at the publication gate**

Report the local commit SHAs, static verification results, files changed, and the remaining Unity results. Publish directly to `Encounters` only after the user explicitly approves publication and the remote branch still matches the reviewed base.
