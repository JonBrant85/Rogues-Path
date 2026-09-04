# Pawn Creation Utility Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Extend `UtilityWindow.cs` with an IMGUI `Pawn Creation` tab that converts a raw HeroEditor4D model prefab in place into a variant of a selectable Pawn base, defaulting to Goblin.

**Architecture:** `UtilityWindow` owns the entire workflow: UI state, input validation, confirmation, preview-scene conversion, model and Pawn reference wiring, asset save, cleanup, and post-save validation. Unity prefab APIs create the variant at the raw model's existing path without changing its `.meta` GUID.

**Tech Stack:** Unity 2022.3.62f2, C#, IMGUI (`EditorGUILayout`), `UnityEditor.PrefabUtility`, `EditorSceneManager`, HeroEditor4D, OldOdin Animazing.

**Spec:** `docs/superpowers/specs/2026-09-04-pawn-creation-utility-design.md`

## Global Constraints

- Implement everything in `Assets/_Rogues Path/Utilities/UtilityWindow.cs`; do not create a Pawn Creation helper or service file.
- Preserve the target prefab path and `.meta` GUID; do not create a second user-facing prefab.
- Use Unity prefab APIs; do not edit serialized prefab YAML.
- Keep shared `Pawn`, `PawnBrain`, root collider, and root-child `UIStatusDisplay` inherited from the selected base.
- Rename the inherited base model to `Base Model (Inactive)` and disable it; Unity 2022.3 does not support removing inherited variant children.
- Keep `Character4D`, `AnimationManager`, `AnimationEvents`, `LayerManager`, `Animator`, `SortingGroup`, and Animazing together on the replacement `Model`.
- Animazing must share a GameObject with the Animator.
- Set the replacement model's Sorting Group to the `Pawns` sorting layer.
- Remove model-root `Collider2D` components; retain exactly one enabled collider on the Pawn root.
- Wire all six internal model references and all three inherited Pawn model references.
- Default the selectable base field to `Assets/_Rogues Path/Pawns/Prefabs/EnemyPawns/Goblin.prefab`.
- Reject null, non-prefab, already-converted, already-variant, self-referential, or dependency-cycle inputs before modification.
- Do not create `PawnData`, configure stats/spells/equipment, batch-convert assets, or modify third-party source.
- Do not add a Unity test assembly; use static source validation plus the Skeleton Archer Unity Editor acceptance test.

---

### Task 1: Implement Pawn Creation entirely in UtilityWindow

**Files:**

- Modify: `Assets/_Rogues Path/Utilities/UtilityWindow.cs`

**Interfaces:**

- Consumes: `pawnModelPrefab`, a raw model `GameObject` prefab asset; `basePawnPrefab`, a selectable Pawn `GameObject` prefab asset.
- Produces: a third `Pawn Creation` tab and the private `CreatePawnVariantInPlace()` workflow.
- Side effect: after validation and confirmation, replaces the selected model prefab contents at the same path with a prefab variant of the selected base.

- [ ] **Step 1: Run the one-file source contract and verify the feature is absent**

Run:

```bash
bash -lc '
set -euo pipefail
file="Assets/_Rogues Path/Utilities/UtilityWindow.cs"
test ! -e "Assets/_Rogues Path/Utilities/PawnCreationUtility.cs"
rg -q '"Pawn Creation"' "$file"
rg -q 'DrawPawnCreationTab' "$file"
rg -q 'CreatePawnVariantInPlace' "$file"
rg -q 'DefaultBasePawnPath' "$file"
rg -q 'NewPreviewScene' "$file"
rg -q 'PrefabUnpackMode.OutermostRoot' "$file"
rg -q 'SaveAsPrefabAsset' "$file"
rg -q 'InactiveBaseModelName' "$file"
rg -q 'SetActive\(false\)' "$file"
rg -q 'sortingLayerName = PawnSortingLayer' "$file"
rg -q 'FindProperty\("animazing"\)' "$file"
rg -q 'ClosePreviewScene' "$file"
'
```

Expected: FAIL on the first `rg` because the `Pawn Creation` tab is not implemented.

- [ ] **Step 2: Add the required imports, constants, and window state**

Add these imports to `UtilityWindow.cs` while retaining its existing imports:

```csharp
using System;
using System.IO;
using _Rogues_Path.Pawns.Scripts;
using Assets.HeroEditor4D.Common.Scripts.CharacterScripts;
using OldOdin;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
```

Add the Pawn Creation constants and required model component list inside `UtilityWindow`:

```csharp
private const string DefaultBasePawnPath =
    "Assets/_Rogues Path/Pawns/Prefabs/EnemyPawns/Goblin.prefab";

private const string PawnSortingLayer = "Pawns";
private const string InactiveBaseModelName = "Base Model (Inactive)";

private static readonly Type[] RequiredModelComponents = {
    typeof(Character4D),
    typeof(AnimationManager),
    typeof(AnimationEvents),
    typeof(LayerManager),
    typeof(Animator),
    typeof(SortingGroup)
};
```

Add the two object fields beside the existing window state:

```csharp
private GameObject pawnModelPrefab;
private GameObject basePawnPrefab;
```

Initialize the default base without replacing an existing selection:

```csharp
private void OnEnable() {
    if (basePawnPrefab == null) {
        basePawnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultBasePawnPath);
    }
}
```

- [ ] **Step 3: Add the Pawn Creation tab UI**

Add `Pawn Creation` between the existing equipment and settings tabs:

```csharp
private readonly string[] tabs = {
    "Equipment Sprite Filler",
    "Pawn Creation",
    "Settings"
};
```

Update the `OnGUI` switch:

```csharp
switch (selectedTab) {
    case 0:
        DrawEquipmentSpriteFillerTab();
        break;
    case 1:
        DrawPawnCreationTab();
        break;
    case 2:
        DrawSettingsTab();
        break;
}
```

Add the tab renderer:

```csharp
private void DrawPawnCreationTab() {
    GUILayout.Label("Pawn Creation", EditorStyles.boldLabel);

    EditorGUILayout.HelpBox(
        "Converts a raw HeroEditor4D model prefab in place into a variant of the selected Pawn base.",
        MessageType.Info);

    pawnModelPrefab = (GameObject)EditorGUILayout.ObjectField(
        new GUIContent("Model Prefab", "Raw model prefab to replace in place."),
        pawnModelPrefab,
        typeof(GameObject),
        false);

    basePawnPrefab = (GameObject)EditorGUILayout.ObjectField(
        new GUIContent("Base Pawn Prefab", "Pawn prefab inherited by the converted variant."),
        basePawnPrefab,
        typeof(GameObject),
        false);

    GUILayout.Space(10);

    using (new EditorGUI.DisabledScope(pawnModelPrefab == null || basePawnPrefab == null)) {
        if (GUILayout.Button("Create Pawn Variant")) {
            CreatePawnVariantInPlace();
        }
    }
}
```

- [ ] **Step 4: Add validation before any destructive work**

Add the workflow entry point. It validates first, then confirms, then delegates to the preview-scene conversion:

```csharp
private void CreatePawnVariantInPlace() {
    if (!TryValidatePawnCreationInputs(
            out string modelPath,
            out string basePath,
            out string error)) {
        ReportPawnCreationFailure(error);
        return;
    }

    bool confirmed = EditorUtility.DisplayDialog(
        "Create Pawn Variant",
        $"Replace '{modelPath}' in place with a variant of '{basePath}'?\n\n" +
        "This prefab-file overwrite cannot be undone reliably.",
        "Create Variant",
        "Cancel");

    if (!confirmed) return;

    try {
        ConvertPawnPrefab(modelPath, basePath);
    }
    catch (Exception exception) {
        Debug.LogException(exception);
        ReportPawnCreationFailure(
            $"Conversion failed: {exception.Message}\n\n" +
            "If Unity wrote the prefab before the failure, restore it through source control.");
    }
}
```

Add persistent-root prefab validation:

```csharp
private static bool TryGetPrefabPath(
    GameObject prefab,
    string label,
    out string path,
    out string error) {
    path = prefab == null ? string.Empty : AssetDatabase.GetAssetPath(prefab);

    if (prefab == null) {
        error = $"{label} is required.";
        return false;
    }

    bool isPrefab = EditorUtility.IsPersistent(prefab)
                    && PrefabUtility.IsPartOfPrefabAsset(prefab)
                    && !string.IsNullOrEmpty(path)
                    && path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)
                    && AssetDatabase.LoadAssetAtPath<GameObject>(path) == prefab;

    if (!isPrefab) {
        error = $"{label} must be a prefab asset root.";
        return false;
    }

    error = string.Empty;
    return true;
}
```

Add raw-model validation:

```csharp
private static bool TryValidatePawnModel(GameObject modelPrefab, out string error) {
    if (PrefabUtility.GetPrefabAssetType(modelPrefab) == PrefabAssetType.Variant) {
        error = "Model Prefab is already a prefab variant.";
        return false;
    }

    if (modelPrefab.GetComponent<Pawn>() != null) {
        error = "Model Prefab already contains a Pawn and appears to be converted.";
        return false;
    }

    foreach (Type componentType in RequiredModelComponents) {
        int count = modelPrefab.GetComponents(componentType).Length;

        if (count != 1) {
            error = $"Model Prefab must contain exactly one {componentType.Name} on its root; found {count}.";
            return false;
        }
    }

    int animazingCount = modelPrefab.GetComponents<Animazing>().Length;

    if (animazingCount > 1) {
        error = $"Model Prefab contains {animazingCount} Animazing components; expected zero or one.";
        return false;
    }

    error = string.Empty;
    return true;
}
```

Add access to Pawn's private serialized Animazing reference and validate the selected base contract:

```csharp
private static bool TryGetPawnAnimazing(Pawn pawn, out Animazing animazing) {
    SerializedObject serializedPawn = new SerializedObject(pawn);
    SerializedProperty property = serializedPawn.FindProperty("animazing");
    animazing = property == null ? null : property.objectReferenceValue as Animazing;
    return animazing != null;
}

private static bool TryValidatePawnBase(GameObject basePrefab, out string error) {
    Pawn pawn = basePrefab.GetComponent<Pawn>();

    if (pawn == null) {
        error = "Base Pawn Prefab must contain Pawn on its root.";
        return false;
    }

    if (pawn.Brain == null || pawn.Brain.gameObject != basePrefab) {
        error = "Base Pawn Prefab must assign a PawnBrain on its root.";
        return false;
    }

    Collider2D[] rootColliders = basePrefab.GetComponents<Collider2D>();

    if (rootColliders.Length != 1 || !rootColliders[0].enabled) {
        error = "Base Pawn Prefab must contain exactly one enabled Collider2D on its root.";
        return false;
    }

    if (pawn.StatusDisplay == null || pawn.StatusDisplay.transform.parent != basePrefab.transform) {
        error = "Base Pawn Prefab must assign a UIStatusDisplay on a direct root child.";
        return false;
    }

    if (pawn.Character == null || pawn.animationManager == null ||
        !TryGetPawnAnimazing(pawn, out Animazing animazing)) {
        error = "Base Pawn Prefab must assign Character, AnimationManager, and Animazing.";
        return false;
    }

    GameObject model = pawn.Character.gameObject;
    bool sharedModel = model.transform.parent == basePrefab.transform
                       && pawn.animationManager.gameObject == model
                       && animazing.gameObject == model;

    if (!sharedModel) {
        error = "Base Pawn Prefab model references must target one direct child.";
        return false;
    }

    error = string.Empty;
    return true;
}
```

Combine the checks and reject self-reference, a missing sorting layer, and dependency cycles:

```csharp
private bool TryValidatePawnCreationInputs(
    out string modelPath,
    out string basePath,
    out string error) {
    basePath = string.Empty;

    if (!TryGetPrefabPath(pawnModelPrefab, "Model Prefab", out modelPath, out error) ||
        !TryGetPrefabPath(basePawnPrefab, "Base Pawn Prefab", out basePath, out error)) {
        return false;
    }

    if (modelPath == basePath) {
        error = "Model Prefab and Base Pawn Prefab cannot be the same asset.";
        return false;
    }

    if (!TryValidatePawnModel(pawnModelPrefab, out error) ||
        !TryValidatePawnBase(basePawnPrefab, out error)) {
        return false;
    }

    if (!Array.Exists(SortingLayer.layers, layer => layer.name == PawnSortingLayer)) {
        error = $"Sorting layer '{PawnSortingLayer}' does not exist.";
        return false;
    }

    if (Array.Exists(
            AssetDatabase.GetDependencies(basePath, true),
            dependency => dependency == modelPath)) {
        error = "The selected base depends on the model prefab and would create a dependency cycle.";
        return false;
    }

    error = string.Empty;
    return true;
}
```

- [ ] **Step 5: Add preview-scene conversion and reference normalization**

Add `ConvertPawnPrefab`. Unpack the raw model instance before parenting it beneath the connected base instance so the output does not nest the asset it replaces:

```csharp
private void ConvertPawnPrefab(string modelPath, string basePath) {
    Scene previewScene = EditorSceneManager.NewPreviewScene();

    try {
        GameObject model = PrefabUtility.InstantiatePrefab(pawnModelPrefab, previewScene) as GameObject;

        if (model == null) throw new InvalidOperationException("Could not instantiate the model prefab.");

        PrefabUtility.UnpackPrefabInstance(
            model,
            PrefabUnpackMode.OutermostRoot,
            InteractionMode.AutomatedAction);

        GameObject pawnRoot = PrefabUtility.InstantiatePrefab(basePawnPrefab, previewScene) as GameObject;

        if (pawnRoot == null) throw new InvalidOperationException("Could not instantiate the base Pawn prefab.");

        Pawn pawn = pawnRoot.GetComponent<Pawn>();
        GameObject inheritedModel = pawn.Character.gameObject;
        inheritedModel.name = InactiveBaseModelName;
        inheritedModel.SetActive(false);
        PrefabUtility.RecordPrefabInstancePropertyModifications(inheritedModel);

        pawnRoot.name = Path.GetFileNameWithoutExtension(modelPath);
        model.name = "Model";
        model.transform.SetParent(pawnRoot.transform, false);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;

        foreach (Collider2D collider in model.GetComponents<Collider2D>()) {
            UnityEngine.Object.DestroyImmediate(collider);
        }

        Character4D character = model.GetComponent<Character4D>();
        AnimationManager animationManager = model.GetComponent<AnimationManager>();
        LayerManager layerManager = model.GetComponent<LayerManager>();
        Animator animator = model.GetComponent<Animator>();
        SortingGroup sortingGroup = model.GetComponent<SortingGroup>();
        Animazing animazing = model.GetComponent<Animazing>() ?? model.AddComponent<Animazing>();

        sortingGroup.sortingLayerName = PawnSortingLayer;

        character.Animator = animator;
        character.AnimationManager = animationManager;
        character.LayerManager = layerManager;
        animationManager.Character = character;
        animationManager.Animator = animator;
        layerManager.SortingGroup = sortingGroup;

        SerializedObject serializedPawn = new SerializedObject(pawn);
        serializedPawn.Update();
        SetPawnObjectReference(serializedPawn, "Character", character);
        SetPawnObjectReference(serializedPawn, "animationManager", animationManager);
        SetPawnObjectReference(serializedPawn, "animazing", animazing);
        serializedPawn.ApplyModifiedPropertiesWithoutUndo();

        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(
            pawnRoot,
            modelPath,
            out bool saveSucceeded);

        if (!saveSucceeded || savedPrefab == null) {
            throw new InvalidOperationException("Unity did not save the Pawn variant.");
        }

        AssetDatabase.ImportAsset(modelPath, ImportAssetOptions.ForceUpdate);
        GameObject importedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);

        if (!TryValidateCreatedPawnVariant(importedPrefab, basePath, out string resultError)) {
            throw new InvalidOperationException(resultError);
        }

        pawnModelPrefab = importedPrefab;
        Selection.activeObject = importedPrefab;
        EditorGUIUtility.PingObject(importedPrefab);
        Debug.Log($"Created Pawn variant at '{modelPath}' from '{basePath}'.", importedPrefab);
    }
    finally {
        if (previewScene.IsValid()) {
            EditorSceneManager.ClosePreviewScene(previewScene);
        }
    }
}

private static void SetPawnObjectReference(
    SerializedObject serializedPawn,
    string propertyName,
    UnityEngine.Object value) {
    SerializedProperty property = serializedPawn.FindProperty(propertyName);

    if (property == null) {
        throw new InvalidOperationException($"Pawn serialized field '{propertyName}' was not found.");
    }

    property.objectReferenceValue = value;
}
```

- [ ] **Step 6: Add post-save validation, success selection, and failure reporting**

Add post-save validation. `GetCorrespondingObjectFromSource` checks the immediate selected base rather than the oldest original prefab in a possible variant chain:

```csharp
private static bool TryValidateCreatedPawnVariant(
    GameObject prefab,
    string basePath,
    out string error) {
    if (prefab == null || PrefabUtility.GetPrefabAssetType(prefab) != PrefabAssetType.Variant) {
        error = "Saved asset is not a prefab variant.";
        return false;
    }

    GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(prefab);

    if (source == null || AssetDatabase.GetAssetPath(source) != basePath) {
        error = "Saved variant does not inherit from the selected base Pawn prefab.";
        return false;
    }

    Pawn pawn = prefab.GetComponent<Pawn>();

    if (pawn == null || pawn.Character == null || pawn.animationManager == null ||
        !TryGetPawnAnimazing(pawn, out Animazing animazing)) {
        error = "Saved Pawn model references are incomplete.";
        return false;
    }

    GameObject model = pawn.Character.gameObject;
    Transform inactiveBaseModel = prefab.transform.Find(InactiveBaseModelName);
    Character4D character = model.GetComponent<Character4D>();
    AnimationManager animationManager = model.GetComponent<AnimationManager>();
    LayerManager layerManager = model.GetComponent<LayerManager>();
    Animator animator = model.GetComponent<Animator>();
    SortingGroup sortingGroup = model.GetComponent<SortingGroup>();

    bool correctHierarchy = model.name == "Model"
                            && model.transform.parent == prefab.transform
                            && model.activeSelf
                            && inactiveBaseModel != null
                            && !inactiveBaseModel.gameObject.activeSelf
                            && pawn.animationManager.gameObject == model
                            && animazing.gameObject == model;

    bool correctInternalReferences = character != null
                                     && animationManager != null
                                     && layerManager != null
                                     && animator != null
                                     && sortingGroup != null
                                     && character.Animator == animator
                                     && character.AnimationManager == animationManager
                                     && character.LayerManager == layerManager
                                     && animationManager.Character == character
                                     && animationManager.Animator == animator
                                     && layerManager.SortingGroup == sortingGroup;

    bool correctBoundary = prefab.GetComponents<Collider2D>().Length == 1
                           && model.GetComponents<Collider2D>().Length == 0
                           && pawn.StatusDisplay != null
                           && pawn.StatusDisplay.transform.parent == prefab.transform
                           && sortingGroup != null
                           && sortingGroup.sortingLayerName == PawnSortingLayer;

    if (!correctHierarchy || !correctInternalReferences || !correctBoundary) {
        error = "Saved Pawn variant failed hierarchy, reference, collider, or sorting-layer validation.";
        return false;
    }

    error = string.Empty;
    return true;
}

private static void ReportPawnCreationFailure(string message) {
    Debug.LogError($"Pawn Creation failed: {message}");
    EditorUtility.DisplayDialog("Pawn Creation Failed", message, "OK");
}
```

- [ ] **Step 7: Run the same source contract and verify the one-file implementation passes**

Run the exact command from Step 1.

Expected: PASS with exit code `0`, and `PawnCreationUtility.cs` remains absent.

- [ ] **Step 8: Run structural checks and review the complete diff**

Run:

```bash
git diff --check
git status --short
git diff -- "Assets/_Rogues Path/Utilities/UtilityWindow.cs"
git diff --name-only
```

Expected:

- No whitespace errors.
- `UtilityWindow.cs` is the only source or asset change.
- `Skeleton Archer.prefab`, `Goblin.prefab`, vendor source, and every `.meta` file remain unchanged.

- [ ] **Step 9: Commit the editor feature locally**

```bash
git add "Assets/_Rogues Path/Utilities/UtilityWindow.cs"
git commit -m "Add Pawn Creation tab"
```

---

### Task 2: Verify the committed implementation and run the Skeleton Archer acceptance gate

**Files:**

- Verify: `Assets/_Rogues Path/Utilities/UtilityWindow.cs`
- Runtime target: `Assets/_Rogues Path/Pawns/Prefabs/EnemyPawns/Skeleton Archer.prefab`
- Runtime base: `Assets/_Rogues Path/Pawns/Prefabs/EnemyPawns/Goblin.prefab`

**Interfaces:**

- Consumes: the committed one-file editor feature from Task 1.
- Produces: a reviewed source commit ready for publication, followed by a user-run Unity Editor conversion and Play Mode result.

- [ ] **Step 1: Rerun verification against the committed source**

Run:

```bash
git status --short
git show --check --stat --oneline HEAD
bash -lc '
set -euo pipefail
file="Assets/_Rogues Path/Utilities/UtilityWindow.cs"
test ! -e "Assets/_Rogues Path/Utilities/PawnCreationUtility.cs"
rg -q '"Pawn Creation"' "$file"
rg -q 'DrawPawnCreationTab' "$file"
rg -q 'CreatePawnVariantInPlace' "$file"
rg -q 'DefaultBasePawnPath' "$file"
rg -q 'NewPreviewScene' "$file"
rg -q 'PrefabUnpackMode.OutermostRoot' "$file"
rg -q 'SaveAsPrefabAsset' "$file"
rg -q 'InactiveBaseModelName' "$file"
rg -q 'SetActive\(false\)' "$file"
rg -q 'sortingLayerName = PawnSortingLayer' "$file"
rg -q 'FindProperty\("animazing"\)' "$file"
rg -q 'ClosePreviewScene' "$file"
'
```

Expected: clean working tree, no commit whitespace errors, one-file source scope, and passing source contract.

- [ ] **Step 2: Publish the reviewed source commit only after user approval**

Fast-forward `Encounters` only if its remote head still matches the implementation branch's reviewed parent. Verify the remote tree and `UtilityWindow.cs` blob after publication. Do not publish a manually edited or pre-converted Skeleton Archer prefab as part of the source commit.

- [ ] **Step 3: Confirm Unity compilation and tab behavior**

In Unity 2022.3.62f2:

1. Allow scripts to recompile and confirm the Console contains no compiler errors.
2. Open `Tools > Rogue's Path Utilities`.
3. Select `Pawn Creation`.
4. Confirm `Base Pawn Prefab` defaults to `Goblin.prefab`.
5. Assign `Skeleton Archer.prefab` to `Model Prefab`.
6. Click `Create Pawn Variant` and cancel once; confirm the prefab is unchanged.
7. Click again, confirm the dialog, and allow the asset to reimport.

Expected: the Project window selects `Skeleton Archer.prefab` and the Console logs one success message.

- [ ] **Step 4: Inspect the converted Skeleton Archer prefab**

Verify in Prefab Mode:

1. The prefab remains at the same path and is a variant of `Goblin.prefab`.
2. The root is named `Skeleton Archer` and inherits `Pawn`, `PawnBrain`, one enabled collider, and `UIStatusDisplay`.
3. The direct children include the inherited status display, disabled `Base Model (Inactive)`, and active replacement `Model`.
4. `Model` retains the Archer's directional hierarchy and scale.
5. `Model` contains exactly one Character4D, AnimationManager, AnimationEvents, LayerManager, Animator, SortingGroup, and Animazing.
6. `Model` has no Collider2D.
7. Sorting Group uses `Pawns`.
8. Character4D, AnimationManager, LayerManager, and Pawn all reference the replacement model's components.
9. The inherited Goblin model is named `Base Model (Inactive)`, remains disabled, and does not render.
10. `git diff -- "Assets/_Rogues Path/Pawns/Prefabs/EnemyPawns/Skeleton Archer.prefab.meta"` is empty.

- [ ] **Step 5: Run the Play Mode acceptance test**

Instantiate or register Skeleton Archer through the project's normal enemy test path, then verify:

1. The Archer renders with its original art and correct facing.
2. Its idle and action animations affect the visible Archer model.
3. Hover shows the status display; mouse exit hides it.
4. Neither hover transition disables the Pawn root or model.
5. The root collider handles mouse interaction and there is no duplicate collision response.
6. The Console contains no missing-reference, missing-script, prefab, Animator, or Animazing errors.

If any check fails, preserve the Console error and prefab Inspector state before changing code. Restore the converted prefab through source control while the tool is corrected.

- [ ] **Step 6: Commit the converted acceptance fixture separately after runtime success**

After the user confirms the Editor and Play Mode gates, commit only the converted prefab if its `.meta` is unchanged:

```bash
git add "Assets/_Rogues Path/Pawns/Prefabs/EnemyPawns/Skeleton Archer.prefab"
git commit -m "Convert Skeleton Archer to Pawn variant"
```

Do not include `Skeleton Archer.prefab.meta` or unrelated Unity-generated changes.
