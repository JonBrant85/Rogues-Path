using System;
using System.Collections.Generic;
using _Rogues_Path._Game;
using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.Pawns;
using _Rogues_Path.UI.Slots;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using HeroEditor.Common.Enums;
using Kryz.CharacterStats;
using Sirenix.OdinInspector;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Rogues_Path.UI.CharacterScreen {
    public class UICharacterScreen : Singleton<UICharacterScreen> {
        private Pawn player;
        public List<UIEquipmentSlot> EquipmentSlots = new();
        public Transform StatsContainer;
        public UICharacterStat StatPrefab;
        public Vector3 PawnPreviewOffset = new Vector3(0, -1.5f, 3f);
        [FoldoutGroup("References"), SerializeField] private Text CharacterNameText;
        [FoldoutGroup("References"), SerializeField] private Text CharacterClassText;
        [FoldoutGroup("References"), SerializeField] private Camera PawnPreviewCamera;
        [FoldoutGroup("Debug"), SerializeField] private Pawn pawnPreview;


        [SerializeField] private StatUIStat stats = new();

        private void Update() {
            // Poll Character stats
            foreach (var kvp in stats) {
                kvp.Value.LabelText.text = kvp.Key.Name;
                kvp.Value.UpdateValue();
            }
        }

        public void SetPlayer(Pawn _player) {
            player = _player;
            
            // Set character name/class
            CharacterNameText.text = player.CharacterName;
            CharacterClassText.text = player.ClassName;
            
            InitializePawnPreview();
            SetupEquipmentSlots();
            ShowCharacterStats();

            // Setup equipment slots
            void SetupEquipmentSlots() {
                foreach (UIEquipmentSlot slot in EquipmentSlots) {
                    slot.Owner = _player;
                    slot.OnAssignEvent.AddListener(OnAssignEventHandler);
                    slot.OnUnassignEvent.AddListener(OnUnassignEventHandler);
                }

                void OnAssignEventHandler(Pawn owner, EquipmentBase equipment) {
                    if (EquipmentDatabase.GetIDByName(equipment.name, out int ID)) {
                        Game.Instance.PlayerEquipment.Add(equipment.EquipType, ID);
                    }
                }

                void OnUnassignEventHandler(Pawn owner, EquipmentBase equipment) {
                    Game.Instance.PlayerEquipment.Remove(equipment.EquipType);
                }
            }

            void InitializePawnPreview() {
                pawnPreview = Instantiate(player, PawnPreviewCamera.transform);
                pawnPreview.transform.localPosition = PawnPreviewOffset;
                foreach (UIEquipmentSlot slot in EquipmentSlots) {
                    slot.Owner = _player;
                    slot.OnAssignEvent.AddListener(OnAssignEventHandler);
                    slot.OnUnassignEvent.AddListener(OnUnassignEventHandler);
                }
                
                void OnAssignEventHandler(Pawn pawn, EquipmentBase equipment) {
                    pawnPreview.TryEquip(equipment, false);
                }
                
                void OnUnassignEventHandler(Pawn pawn, EquipmentBase equipment) {
                    pawnPreview.TryRemoveEquipment(equipment, false);
                }
            }
        }


        private void ShowCharacterStats() {
            BindCharacterStat(player.MaxHealth, "Maximum Health");
            BindCharacterStat(player.Strength, " Strength");
            BindCharacterStat(player.Dexterity, "Dexterity");
            BindCharacterStat(player.Intelligence, "Intelligence");
            BindCharacterStat(player.Speed, "Speed");
        }

        private void BindCharacterStat(CharacterStat stat, string _name) {
            var uiStat = Instantiate(StatPrefab, StatsContainer);
            uiStat.SetCharacterStat(stat, player, _name);
            stats.Add(stat, uiStat);
        }
    }
}