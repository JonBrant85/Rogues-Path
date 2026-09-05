# Encounter Content Polish Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Finish the existing Rest and Treasure encounters with resolved world visuals, atmospheric copy, explicit result feedback, and a Continue acknowledgement.

**Architecture:** Extend the existing `EncounterData`/`UIEncounterWindow` flow instead of adding new controllers. Each runtime encounter retains the visual it instantiated; `UIEncounterWindow` owns a reusable result stage, while Rest and Treasure apply their gameplay effect and resolve their own visual.

**Tech Stack:** Unity 2022.3, C#, Cysharp UniTask, Unity uGUI, Rogue's Path EventBus, FantasyMonsters `Monster` animation component

**Spec:** `docs/superpowers/specs/2026-09-05-encounter-content-polish-design.md`

## Global Constraints

- Keep Rest at 30% maximum-health restoration.
- Keep Treasure at three unique equipment choices with the existing quality weights.
- Result flow is description/action, outcome, Continue, then return to World controls.
- Rest extinguishes its campfire even when the player starts at full health.
- Treasure plays the existing chest animation and leaves the opened chest visible.
- Result strings remain serialized and editable on the Rest and Treasure assets.
- Do not add alternate Rest actions, new reward types, new encounter variants, scenes, or controllers.
- Do not edit either world-visual prefab or any third-party asset.
- Preserve `EquipmentDatabase.Equipment` as the defensive-copy boundary; the current Treasure code does not mutate the database.
- The repository has no project test assembly by choice. Source contracts provide the automated gate; Unity compilation and playtesting provide the runtime gate.

---

### Task 1: Runtime Visual and Shared Result Stage

**Files:**
- Modify: `Assets/_Rogues Path/World/Encounters/EncounterData.cs:7-56`
- Modify: `Assets/_Rogues Path/World/Encounters/UIEncounterWindow.cs:34-186`

**Interfaces:**
- Produces: `protected Transform EncounterData.RuntimeWorldVisual { get; private set; }`
- Produces: `protected internal static UniTask<bool> EncounterData.WaitForConfirmation(Transform bottomBar, Button buttonPrefab, string buttonText)`
- Produces: `public UniTask<bool> UIEncounterWindow.ShowResult(string resultText, string buttonText)`
- Consumes: existing `UIEncounterWindow.bodyText`, `BottomBar`, `ButtonPrefab`, and `ClearEquipmentChoices()`

- [ ] **Step 1: Run the failing shared-lifecycle source contract**

Run:

```bash
set -e
encounter='Assets/_Rogues Path/World/Encounters/EncounterData.cs'
window='Assets/_Rogues Path/World/Encounters/UIEncounterWindow.cs'
rg -Fq 'protected Transform RuntimeWorldVisual { get; private set; }' "$encounter"
rg -Fq 'protected internal static async UniTask<bool> WaitForConfirmation' "$encounter"
rg -Fq 'clicked || confirmationButton == null' "$encounter"
rg -Fq 'public async UniTask<bool> ShowResult(string resultText, string buttonText)' "$window"
rg -Fq 'ClearEquipmentChoices();' "$window"
```

Expected: FAIL at the missing `RuntimeWorldVisual` assertion.

- [ ] **Step 2: Retain the instantiated runtime visual**

In `EncounterData.cs`, add the protected runtime property and assign it only on the instantiated runtime clone:

```csharp
protected Transform RuntimeWorldVisual { get; private set; }

public virtual Transform Initialize(Transform encounterContainer) {
    RuntimeWorldVisual = null;

    if (WorldVisualPrefab == null)
        return null;

    GameObject worldVisual = Instantiate(WorldVisualPrefab, encounterContainer);
    worldVisual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    RuntimeWorldVisual = worldVisual.transform;

    return RuntimeWorldVisual;
}
```

Do not serialize the runtime property. Each `WorldTile` already clones its `EncounterData`, so the reference cannot leak between tiles.

- [ ] **Step 3: Make the existing confirmation helper reusable and destruction-safe**

In `EncounterData.cs`, change the helper visibility from `protected` to `protected internal`. Update its wait and cleanup path so destroying the window hierarchy cannot leave the UniTask waiting forever:

```csharp
protected internal static async UniTask<bool> WaitForConfirmation(
    Transform bottomBar,
    Button buttonPrefab,
    string buttonText) {

    if (bottomBar == null || buttonPrefab == null) {
        Debug.LogError("Encounter confirmation UI is not configured.");
        return false;
    }

    Button confirmationButton = Instantiate(buttonPrefab, bottomBar);
    Text label = confirmationButton.GetComponentInChildren<Text>();

    if (label != null)
        label.text = buttonText;

    bool clicked = false;
    confirmationButton.onClick.AddListener(Confirm);
    confirmationButton.gameObject.SetActive(true);

    await UniTask.WaitUntil(() => clicked || confirmationButton == null);

    if (confirmationButton == null)
        return false;

    confirmationButton.onClick.RemoveListener(Confirm);
    Destroy(confirmationButton.gameObject);

    return true;

    void Confirm() {
        confirmationButton.interactable = false;
        clicked = true;
    }
}
```

Keep `RestEncounter` and `TraversalEncounter` on this same helper; do not introduce a second button lifecycle.

- [ ] **Step 4: Add the reusable result stage**

In `UIEncounterWindow.cs`, add:

```csharp
public async UniTask<bool> ShowResult(string resultText, string buttonText) {
    ClearEquipmentChoices();

    if (bodyText == null) {
        Debug.LogError("Encounter result text is not configured.");
        return false;
    }

    bodyText.text = resultText;

    return await EncounterData.WaitForConfirmation(BottomBar, ButtonPrefab, buttonText);
}
```

This method changes only the body and transient interaction controls. `LoadEncounter` remains responsible for showing and hiding the window and restoring the World bottom bar after `HandleEncounter` finishes.

- [ ] **Step 5: Run the shared-lifecycle contract**

Run the Step 1 command again.

Expected: PASS with exit code 0.

- [ ] **Step 6: Review and commit the shared lifecycle**

Run:

```bash
git diff --check
git diff -- 'Assets/_Rogues Path/World/Encounters/EncounterData.cs' 'Assets/_Rogues Path/World/Encounters/UIEncounterWindow.cs'
git status --short
```

Confirm only the two task files changed, then commit:

```bash
git add 'Assets/_Rogues Path/World/Encounters/EncounterData.cs' 'Assets/_Rogues Path/World/Encounters/UIEncounterWindow.cs'
git commit -m 'Add encounter result stage'
```

---

### Task 2: Rest Resolution and Extinguished Campfire

**Files:**
- Modify: `Assets/_Rogues Path/World/Encounters/RestEncounter.cs:7-19`
- Modify: `Assets/_Rogues Path/World/Encounters/ScriptableObjects/Rest.asset:15-20`

**Interfaces:**
- Consumes: `EncounterData.RuntimeWorldVisual`
- Consumes: `UIEncounterWindow.ShowResult(string resultText, string buttonText)`
- Produces: serialized `ContinueButtonText`, `RestoredResultText`, and `FullHealthResultText`
- Produces: private `void ExtinguishCampfire()`

- [ ] **Step 1: Run the failing Rest source contract**

Run:

```bash
set -e
rest='Assets/_Rogues Path/World/Encounters/RestEncounter.cs'
asset='Assets/_Rogues Path/World/Encounters/ScriptableObjects/Rest.asset'
rg -Fq 'public string ContinueButtonText = "Continue";' "$rest"
rg -Fq 'public string RestoredResultText' "$rest"
rg -Fq 'public string FullHealthResultText' "$rest"
rg -Fq 'ExtinguishCampfire();' "$rest"
rg -Fq 'ParticleSystemStopBehavior.StopEmittingAndClear' "$rest"
rg -Fq 'light.enabled = false;' "$rest"
rg -Fq 'ShowResult(resultText, ContinueButtonText)' "$rest"
rg -Fq 'EncounterTitle: A Quiet Flame' "$asset"
rg -Fq "The fire's warmth settles into your bones" "$asset"
```

Expected: FAIL at the missing `ContinueButtonText` assertion.

- [ ] **Step 2: Add serialized Rest result copy and result flow**

In `RestEncounter.cs`, add these fields after `ButtonText`:

```csharp
public string ContinueButtonText = "Continue";
[TextArea] public string RestoredResultText = "The fire's warmth settles into your bones. You recover {amount} health.";
[TextArea] public string FullHealthResultText = "You rest beside the flames, but your strength is already whole.";
```

Replace `HandleEncounter` with:

```csharp
public override async UniTask HandleEncounter(
    Transform windowContent,
    Transform bottomBar,
    Button buttonPrefab) {

    if (!await WaitForConfirmation(bottomBar, buttonPrefab, ButtonText))
        return;

    float restoredHealth = WorldManager.Instance.HealPlayer(RestoredHealthPercentage);
    ExtinguishCampfire();

    string resultText = restoredHealth > 0f
        ? RestoredResultText.Replace("{amount}", restoredHealth.ToString("0.#"))
        : FullHealthResultText;

    Debug.Log($"Rested for {restoredHealth:0.#} health. Current health={Game.Instance.PlayerCurrentHealth:0.#}.");

    if (UIEncounterWindow.Instance != null)
        await UIEncounterWindow.Instance.ShowResult(resultText, ContinueButtonText);
}
```

The call to `ExtinguishCampfire()` is unconditional after a successful Rest click, so full health consumes the campsite too.

- [ ] **Step 3: Implement visual extinguishing without prefab changes**

Add to `RestEncounter.cs`:

```csharp
private void ExtinguishCampfire() {
    if (RuntimeWorldVisual == null) {
        Debug.LogWarning($"Rest encounter '{name}' has no runtime campfire visual to extinguish.");
        return;
    }

    ParticleSystem[] particleSystems = RuntimeWorldVisual.GetComponentsInChildren<ParticleSystem>(true);
    Light[] lights = RuntimeWorldVisual.GetComponentsInChildren<Light>(true);

    if (particleSystems.Length == 0 && lights.Length == 0)
        Debug.LogWarning($"Rest encounter '{name}' found no fire effects to extinguish.");

    foreach (ParticleSystem particleSystem in particleSystems)
        particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

    foreach (Light light in lights)
        light.enabled = false;
}
```

Do not deactivate `RuntimeWorldVisual`; the log meshes remain visible.

- [ ] **Step 4: Update the Rest asset copy and serialized results**

In `Rest.asset`, preserve the existing script GUID, visual prefab, heal percentage, and action text. Set:

```yaml
  EncounterTitle: A Quiet Flame
  EncounterDescription: A lonely fire burns beside the path, offering a moment of warmth and shelter.
  RestoredHealthPercentage: 0.3
  ButtonText: Rest
  ContinueButtonText: Continue
  RestoredResultText: The fire's warmth settles into your bones. You recover {amount} health.
  FullHealthResultText: You rest beside the flames, but your strength is already whole.
```

- [ ] **Step 5: Run the Rest contract**

Run the Step 1 command again.

Expected: PASS with exit code 0.

- [ ] **Step 6: Review and commit Rest polish**

Run:

```bash
git diff --check
git diff -- 'Assets/_Rogues Path/World/Encounters/RestEncounter.cs' 'Assets/_Rogues Path/World/Encounters/ScriptableObjects/Rest.asset'
git status --short
```

Confirm no prefab or scene changed, then commit:

```bash
git add 'Assets/_Rogues Path/World/Encounters/RestEncounter.cs' 'Assets/_Rogues Path/World/Encounters/ScriptableObjects/Rest.asset'
git commit -m 'Polish rest encounter resolution'
```

---

### Task 3: Treasure Resolution and Opened Chest

**Files:**
- Modify: `Assets/_Rogues Path/World/Encounters/TreasureEncounter.cs:1-106`
- Modify: `Assets/_Rogues Path/World/Encounters/ScriptableObjects/Treasure.asset:15-26`

**Interfaces:**
- Consumes: `EncounterData.RuntimeWorldVisual`
- Consumes: `UIEncounterWindow.ShowResult(string resultText, string buttonText)`
- Consumes: `EquipmentDatabase.Equipment`, which already returns `new List<EquipmentBase>(equipment)`
- Consumes: `Assets.FantasyMonsters.Common.Scripts.Monster.Die()`
- Produces: serialized `ContinueButtonText`, `SuccessResultText`, and `EmptyResultText`
- Produces: private `void OpenChest()` and `UniTask ShowEmptyResult()`

- [ ] **Step 1: Run the failing Treasure source contract and defensive-copy guard**

Run:

```bash
set -e
treasure='Assets/_Rogues Path/World/Encounters/TreasureEncounter.cs'
asset='Assets/_Rogues Path/World/Encounters/ScriptableObjects/Treasure.asset'
database='Assets/_Rogues Path/PawnEquipment/Scripts/EquipmentDatabase.cs'
rg -Fq 'new List<EquipmentBase>(equipment)' "$database"
rg -Fq 'public string ContinueButtonText = "Continue";' "$treasure"
rg -Fq 'public string SuccessResultText' "$treasure"
rg -Fq 'public string EmptyResultText' "$treasure"
rg -Fq 'OpenChest();' "$treasure"
rg -Fq 'chest.Die();' "$treasure"
rg -Fq 'ShowResult(resultText, ContinueButtonText)' "$treasure"
rg -Fq 'await ShowEmptyResult();' "$treasure"
rg -Fq 'EncounterTitle: Abandoned Cache' "$asset"
rg -Fq 'Inside, you find {item} ({quality})' "$asset"
```

Expected: the defensive-copy guard passes, then the command FAILS at the missing `ContinueButtonText` assertion. This records that database mutation is not a current defect.

- [ ] **Step 2: Add serialized Treasure result copy**

Add the third-party namespace at the top of `TreasureEncounter.cs`:

```csharp
using Assets.FantasyMonsters.Common.Scripts;
```

Add after `ButtonText`:

```csharp
public string ContinueButtonText = "Continue";
[TextArea] public string SuccessResultText = "Inside, you find {item} ({quality}). You secure it among your belongings.";
[TextArea] public string EmptyResultText = "The chest opens with a hollow creak. Whatever it once held is long gone.";
```

- [ ] **Step 3: Complete the Treasure success and failure flows**

Keep the current choice construction and quality-weight methods. Replace the early exits and grant tail of `HandleEncounter` with this behavior:

```csharp
if (equipmentChoices.Count == 0) {
    Debug.LogError("Treasure encounter could not find any equipment choices.");
    await ShowEmptyResult();
    return;
}

UIEncounterWindow encounterWindow = UIEncounterWindow.Instance;

if (encounterWindow == null) {
    Debug.LogError("Treasure encounter could not find UIEncounterWindow.");
    return;
}

EquipmentInstanceData selectedEquipment = await encounterWindow.WaitForEquipmentSelection(
    equipmentChoices,
    ButtonText);

if (encounterWindow == null)
    return;

if (selectedEquipment == null
    || !EquipmentDatabase.TryGetByID(selectedEquipment.EquipmentID, out EquipmentBase equipment)) {
    Debug.LogError("Treasure encounter could not resolve a valid equipment selection.");
    await ShowEmptyResult();
    return;
}

Game.Instance.PlayerInventory.Add(selectedEquipment);
EventBus.Raise(new InventoryChanged());
OpenChest();

string resultText = SuccessResultText
    .Replace("{item}", equipment.Name)
    .Replace("{quality}", selectedEquipment.Quality.ToString());

Debug.Log($"Treasure encounter granted {equipment.Name}.");
await encounterWindow.ShowResult(resultText, ContinueButtonText);
```

Do not add another inventory write or another `InventoryChanged` raise in helper methods.

- [ ] **Step 4: Add empty-result and chest-animation helpers**

Add to `TreasureEncounter.cs`:

```csharp
private async UniTask ShowEmptyResult() {
    OpenChest();
    UIEncounterWindow encounterWindow = UIEncounterWindow.Instance;

    if (encounterWindow != null)
        await encounterWindow.ShowResult(EmptyResultText, ContinueButtonText);
}

private void OpenChest() {
    if (RuntimeWorldVisual == null) {
        Debug.LogWarning($"Treasure encounter '{name}' has no runtime chest visual to open.");
        return;
    }

    Monster chest = RuntimeWorldVisual.GetComponent<Monster>();

    if (chest == null) {
        Debug.LogWarning($"Treasure encounter '{name}' found no Monster animation component on its chest visual.");
        return;
    }

    chest.Die();
}
```

The built-in animation runs while the result stage is visible. Do not wait for it and do not modify the third-party prefab or controller.

- [ ] **Step 5: Preserve the defensive-copy selection boundary**

Keep `GetRandomUniqueEquipment` reading through the existing public property:

```csharp
List<EquipmentBase> availableEquipment = EquipmentDatabase.Instance != null
    ? EquipmentDatabase.Instance.Equipment
    : new List<EquipmentBase>();
```

Do not access `EquipmentDatabase.equipment`, add reflection, or create a redundant second list copy. The property already returns a fresh list.

- [ ] **Step 6: Update the Treasure asset copy and serialized results**

In `Treasure.asset`, preserve the existing script GUID, visual prefab, choice count, action text, and quality weights. Set:

```yaml
  EncounterTitle: Abandoned Cache
  EncounterDescription: A weathered chest lies half-hidden beside the road. Its lock has long since surrendered.
  EquipmentChoiceCount: 3
  ButtonText: Select
  ContinueButtonText: Continue
  SuccessResultText: Inside, you find {item} ({quality}). You secure it among your belongings.
  EmptyResultText: The chest opens with a hollow creak. Whatever it once held is long gone.
```

- [ ] **Step 7: Run the Treasure contract**

Run the Step 1 command again.

Expected: PASS with exit code 0.

- [ ] **Step 8: Review and commit Treasure polish**

Run:

```bash
git diff --check
git diff -- 'Assets/_Rogues Path/World/Encounters/TreasureEncounter.cs' 'Assets/_Rogues Path/World/Encounters/ScriptableObjects/Treasure.asset'
git status --short
```

Confirm the shared database and third-party files are unchanged, then commit:

```bash
git add 'Assets/_Rogues Path/World/Encounters/TreasureEncounter.cs' 'Assets/_Rogues Path/World/Encounters/ScriptableObjects/Treasure.asset'
git commit -m 'Polish treasure encounter resolution'
```

---

### Task 4: Full Verification and Unity Handoff

**Files:**
- Verify only; no new file changes

**Interfaces:**
- Consumes: all interfaces produced by Tasks 1-3
- Produces: a reviewed branch ready for Unity compilation and playtesting

- [ ] **Step 1: Run the complete committed source contract**

Run all source-contract commands from Tasks 1-3 against committed `HEAD`.

Expected: all commands PASS with exit code 0.

- [ ] **Step 2: Verify exact branch scope**

Run:

```bash
git diff --check origin/Game-Over...HEAD
git diff --name-only origin/Game-Over...HEAD
git status --short --branch
```

Expected implementation scope, in addition to the approved spec and this plan:

```text
Assets/_Rogues Path/World/Encounters/EncounterData.cs
Assets/_Rogues Path/World/Encounters/RestEncounter.cs
Assets/_Rogues Path/World/Encounters/ScriptableObjects/Rest.asset
Assets/_Rogues Path/World/Encounters/ScriptableObjects/Treasure.asset
Assets/_Rogues Path/World/Encounters/TreasureEncounter.cs
Assets/_Rogues Path/World/Encounters/UIEncounterWindow.cs
```

Confirm there are no changes under `Assets/ThirdParty`, no prefab changes, no scene changes, and no uncommitted files.

- [ ] **Step 3: Review lifecycle and single-effect guarantees**

Inspect the committed diff and confirm:

- Each successful confirmation button removes its listener and destroys itself.
- Destroyed confirmation UI returns `false` instead of waiting forever.
- `LoadEncounter` hides the encounter window only after the result Continue finishes.
- Rest heals exactly once and extinguishes exactly once after the Rest click.
- Full-health Rest still extinguishes and reaches the result stage.
- Treasure adds exactly one `EquipmentInstanceData` and raises exactly one `InventoryChanged` event.
- Empty or invalid Treasure data never changes inventory.
- Empty or invalid Treasure data still opens the chest before displaying its result.
- Missing particle, light, or `Monster` components never block the result stage.
- Equipment selection still reads through `EquipmentDatabase.Equipment` and cannot mutate the private database list.

- [ ] **Step 4: Hand off the Unity runtime gate**

Ask the user to pull/check out the reviewed branch and verify in Unity 2022.3:

1. Compilation and asset import complete without errors.
2. Damaged Rest restores 30% maximum health up to the normal cap, extinguishes the fire, reports the actual amount, and waits for Continue.
3. Full-health Rest changes no health, extinguishes the fire, shows the full-health copy, and waits for Continue.
4. Treasure presents three unique choices, keeps Select disabled until a choice is clicked, grants the selected item once, animates the chest open, shows item name and quality, and waits for Continue.
5. The campfire logs and opened chest remain visible after their windows close.
6. Repeated Treasure encounters do not reduce or reorder the equipment database.

Do not claim runtime completion until the user confirms this Editor gate.
