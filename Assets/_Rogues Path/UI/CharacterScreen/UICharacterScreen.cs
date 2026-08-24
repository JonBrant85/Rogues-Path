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

            CharacterNameText.text = playerData.Name;
            CharacterClassText.text = playerData.ClassName;

            InitializePawnPreview();
            RestoreEquipment();
            SetupEquipmentSlots();
            ShowCharacterStats();


            void InitializePawnPreview() {
                pawnPreview = Instantiate(playerData.Pawn, PawnPreviewCamera.transform);

                pawnPreview.transform.localPosition = PawnPreviewOffset;

                /*
                 * Runtime equipment must ALWAYS start empty.
                 *
                 * Game.PlayerEquipment is authoritative and we'll
                 * materialize fresh live instances from it below.
                 */
                pawnPreview.CurrentEquipment = new EquipmentDictionary();

                /*
                 * Inventory is also reconstructed from authoritative
                 * Game.PlayerInventory.
                 */
                pawnPreview.SyncInventoryFromGameState();

                /*
                 * Give every slot its Pawn before doing anything with
                 * equipment.
                 */
                foreach (var kvp in EquipmentSlots) {
                    kvp.Value.Owner = pawnPreview;
                }
            }


            void RestoreEquipment() {
                /*
                 * Game.PlayerEquipment already contains the correct state.
                 *
                 * We are ONLY creating the live representation required by
                 * this new Pawn preview.
                 */
                foreach (var kvp in Game.Instance.PlayerEquipment) {
                    EquipmentPart equipType = (EquipmentPart)kvp.Key;
                    int equipmentID = kvp.Value;

                    if (!EquipmentDatabase.TryCreateInstance(equipmentID, out EquipmentBase liveEquipment, pawnPreview.transform)) {

                        Debug.LogError($"Failed to create live equipment for " + $"{equipType}, ID {equipmentID}.");

                        continue;
                    }

                    /*
                     * false:
                     *
                     * Do NOT modify Game.PlayerEquipment / PlayerInventory.
                     * We're reconstructing runtime state FROM them.
                     */
                    if (!pawnPreview.TryEquip(liveEquipment, false)) {

                        Debug.LogError($"Failed to restore {liveEquipment.Name} " + $"on {pawnPreview.CharacterName}.");

                        Destroy(liveEquipment.gameObject);
                    }
                }
            }


            void SetupEquipmentSlots() {
                foreach (var kvp in EquipmentSlots) {
                    UIEquipmentSlot slot = kvp.Value;

                    slot.Owner = pawnPreview;

                    Debug.Log(
                        $"UI SLOT: {slot.name} | "
                        + $"Dictionary Key={kvp.Key} | "
                        + $"Slot EquipType={slot.EquipType} | "
                        + $"Pawn Has Key={pawnPreview.CurrentEquipment.ContainsKey(kvp.Key)}");

                    if (pawnPreview.CurrentEquipment.TryGetValue(slot.EquipType, out EquipmentBase liveEquipment)) {

                        bool success = slot.Assign(liveEquipment);

                        Debug.Log(
                            $"CHARACTER SLOT | "
                            + $"Scene={UnityEngine.SceneManagement.SceneManager.GetActiveScene().name} | "
                            + $"Slot={slot.name} | "
                            + $"SlotType={slot.EquipType} | "
                            + $"Equipment={liveEquipment.Name} | "
                            + $"EquipmentType={liveEquipment.EquipType} | "
                            + $"Icon={(liveEquipment.Icon != null ? liveEquipment.Icon.name : "NULL")} | "
                            + $"Success={success} | "
                            + $"SlotEquipment={(slot.Equipment != null ? slot.Equipment.Name : "NULL")} | "
                            + $"EquipToOwner={slot.EquipToOwnerOnAssign}");
                    }
                }

                foreach (var kvp in EquipmentSlots) {
                    UIEquipmentSlot slot = kvp.Value;

                    slot.Owner = pawnPreview;

                    /*
                     * CurrentEquipment contains the fresh LIVE objects we
                     * just restored.
                     *
                     * Assigning that exact live instance will cause the new
                     * UIEquipmentSlot implementation to simply BIND to it.
                     *
                     * It will NOT equip it again.
                     */
                    if (pawnPreview.CurrentEquipment.TryGetValue(kvp.Key, out EquipmentBase liveEquipment)) {

                        if (!slot.Assign(liveEquipment)) {
                            Debug.LogError($"Failed to bind {liveEquipment.Name} " + $"to UI slot {kvp.Key}.");
                        }
                    }

                    /*
                     * Gameplay state is now handled by Pawn.TryEquip() /
                     * TryRemoveEquipment().
                     *
                     * These events should NOT mutate PlayerEquipment again.
                     */
                    slot.OnAssignEvent.AddListener(OnAssignEventHandler);

                    slot.OnUnassignEvent.AddListener(OnUnassignEventHandler);
                }


                void OnAssignEventHandler(Pawn owner, EquipmentBase equipment) {

                    /*
                     * By the time this event fires, Pawn has already
                     * committed the gameplay transaction.
                     *
                     * Just notify other UI.
                     */
                    EventBus.Raise(new InventoryChanged());
                }


                void OnUnassignEventHandler(Pawn owner, EquipmentBase equipment) {

                    /*
                     * Same here: don't touch Game.PlayerEquipment.
                     */
                    EventBus.Raise(new InventoryChanged());
                }
            }


            void ShowCharacterStats() {
                foreach (var kvp in pawnPreview.Stats) {
                    UICharacterStat uiStat = Instantiate(StatPrefab, StatsContainer);

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