using _Rogues_Path._Game;
using _Rogues_Path.Crafting;
using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.Pawns.Scripts;
using _Rogues_Path.UI.Slots;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using DuloGames.UI;
using HeroEditor.Common.Enums;
using Sirenix.OdinInspector;
using UnityEngine;
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
        [FoldoutGroup("References"), SerializeField] private EquipmentModifierDatabase ModifierDatabase;
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
                    EquipmentInstanceData instanceData = kvp.Value;

                    if (!EquipmentDatabase.TryCreateInstance(instanceData, ModifierDatabase, out EquipmentBase liveEquipment, pawnPreview.transform)) {

                        Debug.LogError($"Failed to create live equipment for " + $"{equipType}, ID {instanceData.EquipmentID}.");

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

                    EquipmentBase matchingEquipment = null;

                    foreach (var equipmentKvp in pawnPreview.CurrentEquipment) {
                        Assets.HeroEditor4D.Common.Scripts.Enums.EquipmentPart equipType = equipmentKvp.Key;

                        if (!slot.AcceptedEquipTypes.Accepts(equipType))
                            continue;

                        matchingEquipment = equipmentKvp.Value;
                        break;
                    }

                    if (matchingEquipment != null) {
                        if (!slot.Assign(matchingEquipment)) {
                            Debug.LogError($"Failed to bind {matchingEquipment.Name} " + $"to UI slot {slot.name}.");
                        }
                    }

                    slot.OnAssignEvent.AddListener(OnAssignEventHandler);
                    slot.OnUnassignEvent.AddListener(OnUnassignEventHandler);
                }

                void OnAssignEventHandler(Pawn owner, EquipmentBase equipment) {
                    EventBus.Raise(new InventoryChanged());
                }

                void OnUnassignEventHandler(Pawn owner, EquipmentBase equipment) {
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