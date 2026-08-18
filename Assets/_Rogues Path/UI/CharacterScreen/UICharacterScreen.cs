using System;
using System.Collections.Generic;
using _Rogues_Path._Game;
using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.Pawns;
using _Rogues_Path.Pawns.Scripts;
using _Rogues_Path.UI.Slots;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using DuloGames.UI;
using HeroEditor.Common.Enums;
using Kryz.CharacterStats;
using Sirenix.OdinInspector;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Rogues_Path.UI.CharacterScreen {
    public class UICharacterScreen : Singleton<UICharacterScreen> {
        public Vector3 PawnPreviewOffset = new Vector3(0, -1.5f, 3f);

        [FoldoutGroup("References"), SerializeField] Transform StatsContainer;
        [FoldoutGroup("References"), SerializeField] public UICharacterStat StatPrefab;
        [FoldoutGroup("References"), SerializeField] private Text CharacterNameText;
        [FoldoutGroup("References"), SerializeField] private Text CharacterClassText;
        [FoldoutGroup("References"), SerializeField] private Camera PawnPreviewCamera;
        [FoldoutGroup("References"), SerializeField] private UIWindow Window;

        [SerializeField] private EquipmentPartUIEquipSlotDictionary EquipmentSlots = new();
        [SerializeField] private StatUIStat stats = new();

        [FoldoutGroup("Debug"), SerializeField] private Pawn pawnPreview;

        private PawnData playerData;

        private void Update() {
            // Poll Character stats
            foreach (var kvp in stats) {
                kvp.Value.LabelText.text = kvp.Key.Name;
                kvp.Value.UpdateValue();
            }
        }

        public void SetPlayer(PawnData _playerData) {
            playerData = _playerData;

            // Set character name/class
            CharacterNameText.text = playerData.Name;
            CharacterClassText.text = playerData.ClassName;

            InitializePawnPreview();
            SetupEquipmentSlots();
            ShowCharacterStats();

            void InitializePawnPreview() {
                // Instantiate PawnPreview and move it to appropriate Camera range
                pawnPreview = Instantiate(playerData.Pawn, PawnPreviewCamera.transform);
                pawnPreview.transform.localPosition = PawnPreviewOffset;
                
                // Clear equipment. SetupEquipmentSlots will check for existing equipment and assign it
                pawnPreview.CurrentEquipment = new EquipmentDictionary(playerData.Pawn.CurrentEquipment);

                // Assign Owner to each slot
                foreach (var kvp in EquipmentSlots) {
                    kvp.Value.Owner = pawnPreview;
                }
            }

            // Setup equipment slots
            void SetupEquipmentSlots() {
                // Assign PlayerEquipment/Owner to EquipmentSlots. Do this before adding listeners to avoid problems
                foreach (var kvp in EquipmentSlots) {
                    if (Game.Instance.PlayerEquipment.ContainsKey(kvp.Key)) {
                        var ID = Game.Instance.PlayerEquipment[kvp.Key];

                        if (EquipmentDatabase.TryGetByID(ID, out EquipmentBase equipment)) {
                            kvp.Value.Assign(equipment);
                        }
                        else {
                            Debug.Log($"Failed to get equipment from database");
                        }
                    }

                    kvp.Value.OnAssignEvent.AddListener(OnAssignEventHandler);
                    kvp.Value.OnUnassignEvent.AddListener(OnUnassignEventHandler);
                    kvp.Value.Owner = pawnPreview;
                }

                void OnAssignEventHandler(Pawn owner, EquipmentBase equipment) {
                    // If slot is occupied, try moving to inventory
                    if (Game.Instance.PlayerEquipment.TryGetValue(equipment.EquipType, out int equippedID)) {
                        if (EquipmentDatabase.TryGetByID(equippedID, out EquipmentBase equippedItem)) {
                            // We have the equipped item and its ID, now we check if we can remove it and add it to inventory. If not, return
                            if (!pawnPreview.TryRemoveEquipment(equippedItem) || !pawnPreview.TryAddToInventory(equippedItem)) {
                                Debug.Log($"Failed to unequip or add to inventory: {equipment.Name}");
                                return;
                            }

                            // If the slot is clear, take it
                            Game.Instance.PlayerEquipment.Add(equipment.EquipType, EquipmentDatabase.Instance.Equipment.IndexOf(equipment));

                            // Raise an InventoryChanged event
                            EventBus.Raise(new InventoryChanged());
                        }
                        else {
                            Debug.Log($"Failed to find equipment By ID: {equippedID}");
                        }
                    }
                    else {
                        // If the slot is clear, take it
                        Game.Instance.PlayerEquipment.Add(equipment.EquipType, EquipmentDatabase.Instance.Equipment.IndexOf(equipment));
                    }
                }

                void OnUnassignEventHandler(Pawn owner, EquipmentBase equipment) {
                    Game.Instance.PlayerEquipment.Remove(equipment.EquipType);
                }
            }

            void ShowCharacterStats() {
                foreach (var kvp in pawnPreview.Stats) {
                    var uiStat = Instantiate(StatPrefab, StatsContainer);
                    uiStat.SetCharacterStat(kvp.Value, kvp.Key.name);

                    stats.Add(kvp.Value, uiStat);
                }
            }
        }

        public static void Show() {
            Instance.Window.Show();
        }

        public static void Hide() {
            Instance.Window.Hide();
        }
    }
}