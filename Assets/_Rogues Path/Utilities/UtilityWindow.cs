#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.Pawns.Scripts;
using Assets.HeroEditor4D.Common.Scripts.Collections;
using Assets.HeroEditor4D.Common.Scripts.CharacterScripts;
using Assets.HeroEditor4D.Common.Scripts.Data;
using OldOdin;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace _Rogues_Path.Utilities {
    public class UtilityWindow : EditorWindow {
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

        private EquipmentBase equipment;
        private SpriteCollection spriteCollection;
        private IconCollection iconCollection;
        private string spriteName = "Default Sprite Name";
        private GameObject pawnModelPrefab;
        private GameObject basePawnPrefab;

        private int selectedTab = 0;
        private readonly string[] tabs = {
            "Equipment Sprite Filler",
            "Pawn Creation",
            "Settings"
        };

        [MenuItem("Tools/Rogue's Path Utilities")]
        public static void ShowWindow() {
            GetWindow<UtilityWindow>("My Custom Window");
        }

        private void OnEnable() {
            if (basePawnPrefab == null) {
                basePawnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultBasePawnPath);
            }
        }

        private void OnGUI() {
            GUILayout.Label("Player Settings", EditorStyles.boldLabel);

            selectedTab = GUILayout.Toolbar(selectedTab, tabs);

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
        }

        private void DrawEquipmentSpriteFillerTab() {
            spriteName = EditorGUILayout.TextField(new GUIContent("ItemSprite.Id", "Given by 4D MegaPack scene"), spriteName);

            GUILayout.Space(10);

            equipment = (EquipmentBase)EditorGUILayout.ObjectField("Equipment", equipment, typeof(EquipmentBase), false);
            spriteCollection = (SpriteCollection)EditorGUILayout.ObjectField("Sprite Collection", spriteCollection, typeof(SpriteCollection), false);
            iconCollection = (IconCollection)EditorGUILayout.ObjectField("Icon Collection", iconCollection, typeof(IconCollection));
            GUILayout.Space(10);

            if (GUILayout.Button("Apply")) {
                ApplySpritesToEquipment();
            }
        }

        private void ApplySpritesToEquipment() {
            if (equipment == null || spriteCollection == null)
                return;

            List<List<ItemSprite>> listOfLists = new() {
                spriteCollection.Armor,
                spriteCollection.MeleeWeapon1H,
                spriteCollection.MeleeWeapon2H,
                spriteCollection.Bow,
                spriteCollection.Crossbow,
                spriteCollection.Firearm1H,
                spriteCollection.Firearm2H,
                spriteCollection.Shield,
                spriteCollection.Back,
                spriteCollection.Throwable,
                spriteCollection.Supplies
            };

            List<ItemSprite> flatList = listOfLists.SelectMany(x => x).ToList();
            ItemSprite itemSprite = flatList.FirstOrDefault(sprite => sprite.Id == spriteName);

            if (itemSprite == null) {
                Debug.Log($"Couldn't find ItemSprite.Id: {spriteName}");

                return;
            }

            Undo.RecordObject(equipment, "Apply Equipment Sprites");

            equipment.ItemSprite.Name = itemSprite.Name;
            equipment.ItemSprite.Id = itemSprite.Id;
            equipment.ItemSprite.Edition = itemSprite.Edition;
            equipment.ItemSprite.Collection = itemSprite.Collection;
            equipment.ItemSprite.Path = itemSprite.Path;
            equipment.ItemSprite.Sprite = itemSprite.Sprite;
            equipment.ItemSprite.Sprites = new List<Sprite>(itemSprite.Sprites);
            equipment.ItemSprite.Tags = new List<string>(itemSprite.Tags);
            equipment.ItemSprite.Meta = itemSprite.Meta;

            equipment.Icon = iconCollection.Icons.FirstOrDefault(icon => icon.Id == spriteName)?.Sprite;

            EditorUtility.SetDirty(equipment);

            if (PrefabUtility.IsPartOfPrefabAsset(equipment)) {
                PrefabUtility.SavePrefabAsset(equipment.transform.root.gameObject);
            }

            AssetDatabase.SaveAssets();

            Debug.Log($"Applied and saved sprites to {equipment.Name}.");
        }

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

            if (Array.IndexOf(AssetDatabase.GetDependencies(basePath, true), modelPath) >= 0) {
                error = "The selected base depends on the model prefab and would create a dependency cycle.";
                return false;
            }

            error = string.Empty;
            return true;
        }

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

        private static bool TryGetPawnAnimazing(Pawn pawn, out Animazing animazing) {
            SerializedObject serializedPawn = new SerializedObject(pawn);
            SerializedProperty property = serializedPawn.FindProperty("animazing");
            animazing = property == null ? null : property.objectReferenceValue as Animazing;
            return animazing != null;
        }

        private void ConvertPawnPrefab(string modelPath, string basePath) {
            Scene previewScene = EditorSceneManager.NewPreviewScene();

            try {
                GameObject model = PrefabUtility.InstantiatePrefab(pawnModelPrefab, previewScene) as GameObject;

                if (model == null) {
                    throw new InvalidOperationException("Could not instantiate the model prefab.");
                }

                PrefabUtility.UnpackPrefabInstance(
                    model,
                    PrefabUnpackMode.OutermostRoot,
                    InteractionMode.AutomatedAction);

                GameObject pawnRoot = PrefabUtility.InstantiatePrefab(basePawnPrefab, previewScene) as GameObject;

                if (pawnRoot == null) {
                    throw new InvalidOperationException("Could not instantiate the base Pawn prefab.");
                }

                Pawn pawn = pawnRoot.GetComponent<Pawn>();
                GameObject inheritedModel = pawn.Character.gameObject;
                inheritedModel.name = InactiveBaseModelName;
                inheritedModel.SetActive(false);
                PrefabUtility.RecordPrefabInstancePropertyModifications(inheritedModel);

                pawnRoot.name = Path.GetFileNameWithoutExtension(modelPath);
                PrefabUtility.RecordPrefabInstancePropertyModifications(pawnRoot);
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
            Collider2D[] rootColliders = prefab.GetComponents<Collider2D>();

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

            bool correctBoundary = rootColliders.Length == 1
                                   && rootColliders[0].enabled
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

        private void DrawSettingsTab() {}
    }
}
#endif
