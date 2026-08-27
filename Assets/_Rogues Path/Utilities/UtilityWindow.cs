#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using _Rogues_Path.Equipment.Scripts;
using Assets.HeroEditor4D.Common.Scripts.Collections;
using Assets.HeroEditor4D.Common.Scripts.Data;
using UnityEditor;
using UnityEngine;

namespace _Rogues_Path.Utilities {
    public class UtilityWindow : EditorWindow {
        private EquipmentBase equipment;
        private SpriteCollection spriteCollection;
        private IconCollection iconCollection;
        private string spriteName = "Default Sprite Name";

        private int selectedTab = 0;
        private readonly string[] tabs = {
            "Equipment Sprite Filler",
            "Settings"
        };

        [MenuItem("Tools/Rogue's Path Utilities")]
        public static void ShowWindow() {
            GetWindow<UtilityWindow>("My Custom Window");
        }

        private void OnGUI() {
            GUILayout.Label("Player Settings", EditorStyles.boldLabel);

            selectedTab = GUILayout.Toolbar(selectedTab, tabs);

            switch (selectedTab) {
                case 0:
                    DrawEquipmentSpriteFillerTab();
                    break;
                case 1:
                    DrawSettingsTab();
                    break;
                default:
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

        private void DrawSettingsTab() {}
    }
}
#endif