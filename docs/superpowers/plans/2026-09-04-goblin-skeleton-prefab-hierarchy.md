# Goblin and Skeleton Prefab Hierarchy Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make `Goblin.prefab` the canonical enemy Pawn base with a self-contained model child, then simplify `Skeleton Variant.prefab` to inherit gameplay behavior and replace only that model.

**Architecture:** The Pawn root owns gameplay, hover detection, and the status-display child. Each model child owns every rendering and animation component, including Animazing and Animator on the same GameObject. The Skeleton remains a Goblin prefab variant and overrides only `Pawn.Character`, `Pawn.animationManager`, and `Pawn.animazing` for presentation.

**Tech Stack:** Unity 2022.3 prefab YAML, HeroEditor4D, Old Odin Animazing, Unity Animator, Unity `CapsuleCollider2D`

**Spec:** `docs/superpowers/specs/2026-09-04-goblin-skeleton-prefab-hierarchy-design.md`

## Global Constraints

- `Skeleton Variant.prefab` must remain a variant of `Goblin.prefab`.
- `UIStatusDisplay` must remain a distinct child of the Pawn root and must never control the active state of the Pawn root or model.
- Exactly one `CapsuleCollider2D` must remain on the Pawn root.
- Animazing and Animator must share the same model GameObject.
- Preserve existing component file IDs when moving Goblin components.
- Do not modify scripts, animation clips, Animator controllers, sprites, scenes, PawnData, encounters, or vendor source.
- Implementation changes exactly `Goblin.prefab` and `Skeleton Variant.prefab`.
- Do not add Unity test files or a test assembly; use temporary serialized-asset validation plus the user's Unity playtest.

---

### Task 1: Establish the canonical Goblin model boundary and migrate the Skeleton variant atomically

**Files:**
- Modify: `Assets/_Rogues Path/Pawns/Prefabs/EnemyPawns/Goblin.prefab:2046,15469-15903`
- Modify: `Assets/_Rogues Path/Pawns/Prefabs/EnemyPawns/Skeleton Variant.prefab:1057-1396,19194-21276`
- Validate: temporary read-only Python checks executed from the repository root

**Interfaces:**
- Consumes: the approved root/model ownership contract in the design specification
- Produces: a Goblin base and Skeleton variant whose Pawn references target their respective active model components

- [ ] **Step 1: Run the pre-change validation and confirm it fails for the intended reasons**

Run:

```bash
python3 - <<'PY'
from pathlib import Path

goblin = Path("Assets/_Rogues Path/Pawns/Prefabs/EnemyPawns/Goblin.prefab").read_text()
skeleton = Path("Assets/_Rogues Path/Pawns/Prefabs/EnemyPawns/Skeleton Variant.prefab").read_text()

model_id = "1251938076275882167"
goblin_root_id = "7747903837301588595"
skeleton_model_id = "274843070243413644"

goblin_visual_components = {
    "Character4D": "5658223203294176127",
    "AnimationManager": "7469095601690709282",
    "AnimationEvents": "733481587254358207",
    "LayerManager": "5340896301592536223",
    "Animator": "8258263592897685535",
    "SortingGroup": "8355299695700497459",
    "Animazing": "3222362884811208898",
}

failures = []
for name, component_id in goblin_visual_components.items():
    marker = f"--- !u!"
    start = goblin.find(f"&{component_id}\\n")
    end = goblin.find("\\n--- !u!", start + 1)
    block = goblin[start:end if end != -1 else None]
    if f"m_GameObject: {{fileID: {model_id}}}" not in block:
        failures.append(f"Goblin {name} is not owned by Model")

if "m_GameObject: {fileID: 274843070243413644}" not in skeleton:
    failures.append("Skeleton model missing")

animazing_script = "guid: e810158de0072a44bb385d970936cef2"
skeleton_model_animazing = (
    animazing_script in skeleton
    and f"m_GameObject: {{fileID: {skeleton_model_id}}}" in skeleton
)
if not skeleton_model_animazing:
    failures.append("Skeleton model has no Animazing")

if not failures:
    raise SystemExit("Unexpected pass: canonical hierarchy already exists")

raise SystemExit("RED — " + "; ".join(failures))
PY
```

Expected: non-zero exit containing `Goblin Character4D is not owned by Model` and `Skeleton model has no Animazing`.

- [ ] **Step 2: Move the Goblin presentation components into its existing `Model` child**

In `Goblin.prefab`, change the `Model` GameObject's component list from Transform-only to:

```yaml
  m_Component:
  - component: {fileID: 6094806263217997431}
  - component: {fileID: 5658223203294176127}
  - component: {fileID: 7469095601690709282}
  - component: {fileID: 733481587254358207}
  - component: {fileID: 5340896301592536223}
  - component: {fileID: 8258263592897685535}
  - component: {fileID: 8355299695700497459}
  - component: {fileID: 3222362884811208898}
```

Change each of those seven component documents from:

```yaml
  m_GameObject: {fileID: 7747903837301588595}
```

to:

```yaml
  m_GameObject: {fileID: 1251938076275882167}
```

Remove those seven component IDs from the Goblin root's `m_Component` list. Retain exactly the root Transform, PawnBrain, Pawn, and root collider:

```yaml
  m_Component:
  - component: {fileID: 3112571022494191160}
  - component: {fileID: -2408596562942195320}
  - component: {fileID: 7463688952052727956}
  - component: {fileID: 4174797735423078041}
```

Do not change component file IDs or the existing reference fields. Because the IDs remain stable, `Pawn.Character`, `Pawn.animationManager`, `Pawn.animazing`, `Character4D`, `AnimationManager`, and `LayerManager` continue pointing to the same components after their owner becomes `Model`.

- [ ] **Step 3: Give the Skeleton model its own Animazing and remove its duplicate collider**

Use the unused component file ID `6892981147734432195` for the Skeleton Animazing component.

Change the Skeleton model GameObject's component list to:

```yaml
  m_Component:
  - component: {fileID: 2433260411807272743}
  - component: {fileID: 6600507933509415295}
  - component: {fileID: 2148911942040026796}
  - component: {fileID: 638250869984325541}
  - component: {fileID: 5792665087612562442}
  - component: {fileID: 384220187860554737}
  - component: {fileID: 7613717068248886806}
  - component: {fileID: 6892981147734432195}
```

Add this component document next to the other Skeleton model components:

```yaml
--- !u!114 &6892981147734432195
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 274843070243413644}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: e810158de0072a44bb385d970936cef2, type: 3}
  m_Name:
  m_EditorClassIdentifier:
```

Delete the complete `CapsuleCollider2D` document with file ID `3748146940366009186`. The variant inherits the existing root collider `4174797735423078041` from Goblin.

- [ ] **Step 4: Simplify Skeleton inheritance overrides**

In the Skeleton variant's `PrefabInstance.m_Modification.m_Modifications` list:

- Delete all 12 override entries targeting base `Character4D` file ID `5658223203294176127`.
- Delete the one override entry targeting base `AnimationManager` file ID `7469095601690709282`.
- Delete all 158 override entries targeting base `LayerManager` file ID `5340896301592536223`.
- Retain the override that maps base `Pawn.Character` to Skeleton component `6600507933509415295`.
- Retain the override that maps base `Pawn.animationManager` to Skeleton component `2148911942040026796`.
- Add this override immediately beside the other Pawn model-reference overrides:

```yaml
    - target: {fileID: 7463688952052727956, guid: 4b4c34eb6535dc3499955f7d7f31706f, type: 3}
      propertyPath: animazing
      value:
      objectReference: {fileID: 6892981147734432195}
```

Replace the obsolete removed-component entry with an empty list:

```yaml
    m_RemovedComponents: []
```

Retain the removed GameObject entry for base `Model` file ID `1251938076275882167` and the added Skeleton model GameObject. Retain the Skeleton identity overrides and all root gameplay configuration.

- [ ] **Step 5: Run the post-change serialized-prefab validation**

Run:

```bash
python3 - <<'PY'
from pathlib import Path
import re

goblin = Path("Assets/_Rogues Path/Pawns/Prefabs/EnemyPawns/Goblin.prefab").read_text()
skeleton = Path("Assets/_Rogues Path/Pawns/Prefabs/EnemyPawns/Skeleton Variant.prefab").read_text()

model_id = "1251938076275882167"
root_id = "7747903837301588595"
skeleton_model_id = "274843070243413644"
skeleton_animazing_id = "6892981147734432195"
base_guid = "4b4c34eb6535dc3499955f7d7f31706f"

moved = {
    "5658223203294176127",
    "7469095601690709282",
    "733481587254358207",
    "5340896301592536223",
    "8258263592897685535",
    "8355299695700497459",
    "3222362884811208898",
}

def document(text, file_id):
    start = text.find(f"&{file_id}\\n")
    if start == -1:
        raise AssertionError(f"Missing document {file_id}")
    end = text.find("\\n--- !u!", start + 1)
    return text[start:end if end != -1 else None]

for file_id in moved:
    assert f"m_GameObject: {{fileID: {model_id}}}" in document(goblin, file_id)

root = document(goblin, root_id)
for file_id in moved:
    assert f"component: {{fileID: {file_id}}}" not in root
assert "component: {fileID: 4174797735423078041}" in root

model = document(goblin, model_id)
for file_id in moved:
    assert f"component: {{fileID: {file_id}}}" in model

skeleton_model = document(skeleton, skeleton_model_id)
assert f"component: {{fileID: {skeleton_animazing_id}}}" in skeleton_model
assert "component: {fileID: 3748146940366009186}" not in skeleton_model
assert f"m_GameObject: {{fileID: {skeleton_model_id}}}" in document(skeleton, skeleton_animazing_id)
assert "guid: e810158de0072a44bb385d970936cef2" in document(skeleton, skeleton_animazing_id)
assert "--- !u!70 &3748146940366009186" not in skeleton

for obsolete in (
    "5658223203294176127",
    "7469095601690709282",
    "5340896301592536223",
):
    assert not re.search(
        rf"target: \{{fileID: {obsolete}, guid: {base_guid}",
        skeleton,
    )

assert "m_RemovedComponents: []" in skeleton
assert "m_RemovedGameObjects:\n    - {fileID: 1251938076275882167" in skeleton
assert "propertyPath: Character\n      value: \n      objectReference: {fileID: 6600507933509415295}" in skeleton
assert "propertyPath: animationManager\n      value: \n      objectReference: {fileID: 2148911942040026796}" in skeleton
assert f"propertyPath: animazing\n      value: \n      objectReference: {{fileID: {skeleton_animazing_id}}}" in skeleton
assert "m_SourcePrefab: {fileID: 100100000, guid: 4b4c34eb6535dc3499955f7d7f31706f, type: 3}" in skeleton

changed = {
    line.strip()
    for line in __import__("subprocess").check_output(
        ["git", "diff", "--name-only"], text=True
    ).splitlines()
}
assert changed == {
    "Assets/_Rogues Path/Pawns/Prefabs/EnemyPawns/Goblin.prefab",
    "Assets/_Rogues Path/Pawns/Prefabs/EnemyPawns/Skeleton Variant.prefab",
}

print("PASS: canonical Goblin model and Skeleton variant inheritance verified")
PY

git diff --check
```

Expected: both commands exit `0`; the Python command prints `PASS: canonical Goblin model and Skeleton variant inheritance verified`.

- [ ] **Step 6: Review the exact prefab diff**

Run:

```bash
git diff --stat
git diff -- "Assets/_Rogues Path/Pawns/Prefabs/EnemyPawns/Goblin.prefab"
git diff -- "Assets/_Rogues Path/Pawns/Prefabs/EnemyPawns/Skeleton Variant.prefab"
```

Confirm:

- Only the two approved prefab files changed.
- Goblin component documents retain their original file IDs.
- Goblin directional objects and sprite references are unchanged.
- Skeleton identity, sprite references, controller, and directional objects are unchanged.
- Exactly 171 obsolete variant override entries are removed.
- No source asset GUID changes.

- [ ] **Step 7: Commit the atomic prefab migration**

```bash
git add \
  "Assets/_Rogues Path/Pawns/Prefabs/EnemyPawns/Goblin.prefab" \
  "Assets/_Rogues Path/Pawns/Prefabs/EnemyPawns/Skeleton Variant.prefab"
git commit -m "Normalize enemy prefab model hierarchy"
```

### Task 2: Verify integration and prepare the Unity runtime gate

**Files:**
- Verify only: `Assets/_Rogues Path/Pawns/Prefabs/EnemyPawns/Goblin.prefab`
- Verify only: `Assets/_Rogues Path/Pawns/Prefabs/EnemyPawns/Skeleton Variant.prefab`

**Interfaces:**
- Consumes: the atomic prefab migration from Task 1
- Produces: a reviewed commit ready for the user's Unity import and playtest

- [ ] **Step 1: Re-run the complete post-change validation from Task 1, Step 5 against `HEAD`**

Expected: every assertion passes and `git diff --check` exits `0`.

- [ ] **Step 2: Verify commit scope and worktree state**

Run:

```bash
git show --stat --oneline HEAD
git diff HEAD^ HEAD --name-only
git status --short
```

Expected:

- The commit changes exactly the two enemy prefabs.
- The worktree is clean.

- [ ] **Step 3: Record the Unity playtest checklist in the handoff**

Require the user to verify in Unity:

1. Unity imports both prefabs without missing-script or broken-prefab warnings.
2. Goblin and Skeleton spawn with their correct models.
3. Both face left as enemies.
4. Jab visibly animates both models.
5. At least one Animazing-driven command visibly animates the rendered model.
6. Hover shows only `UIStatusDisplay`; mouse exit hides it.
7. Status-display toggling never disables the Pawn or model.
8. Damage and death animations still affect the rendered model.
9. Each runtime enemy has one effective collider and one active Animator.

Do not claim Unity compilation, prefab import, or runtime behavior passed until the user completes this checklist.
