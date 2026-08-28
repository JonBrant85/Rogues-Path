using System;
using System.Collections.Generic;
using _Rogues_Path._Game;
using _Rogues_Path.Crafting;
using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.Pawns;
using _Rogues_Path.UI.CharacterScreen;
using _Rogues_Path.UI.CraftingWindow;
using _Rogues_Path.UI.InventoryWindow;
using _Rogues_Path.UI.Slots;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using DuloGames.UI;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace _Rogues_Path.UI.RewardsScreen {
    public class UIRewardsScreen : Singleton<UIRewardsScreen> {
        public Vector3 PawnCameraOffset = new Vector3(0, -1.5f, 3.0f);
        [FoldoutGroup("References"), SerializeField] private UIWindow Window;
        [FoldoutGroup("References"), SerializeField] private Button AcceptButton;
        [FoldoutGroup("References"), SerializeField] private UIEquipmentSlot EquipSlotPrefab;
        [FoldoutGroup("References"), SerializeField] private Transform SlotsContainer;
        [FoldoutGroup("References"), SerializeField] private Button ContinueButton;

        private UIBlackOverlay blackOverlay;

        private void Awake() {
            PrepareUI();
            PopulateRewards();
            Show();
            UICharacterScreen.Instance.SetPlayer(Game.Instance.PlayerData);

            void PrepareUI() {
                Instance.AcceptButton.onClick.AddListener(AcceptButtonClicked);
            }

            void AcceptButtonClicked() {
                AcceptButton.interactable = false;

                CollectRewards();

                void CollectRewards() {
                    foreach (int ID in Game.Instance.PendingRewards) {
                        EquipmentInstanceData instanceData = new(ID);

                        Game.Instance.PlayerInventory.Add(instanceData);
                    }

                    Game.Instance.PendingRewards.Clear();

                    EventBus.Raise(new InventoryChanged());
                }

                Hide();

                UICharacterScreen.Show();
                UICraftingWindow.Show();
                UIInventoryWindow.Show();

                ContinueButton.gameObject.SetActive(true);
            }

            void PopulateRewards() {
                foreach (var ID in Game.Instance.PendingRewards) {
                    var equipSlot = Instantiate(Instance.EquipSlotPrefab, Instance.SlotsContainer);

                    if (EquipmentDatabase.TryGetByID(ID, out EquipmentBase equipment)) {
                        bool success = equipSlot.Assign(equipment);
                    }
                    else {
                        Debug.Log($"Failed");
                    }

                    equipSlot.gameObject.SetActive(true);
                }
            }
        }


        public static void Show() {
            Instance.Window.Show();
            Instance.blackOverlay = UIBlackOverlayManager.Instance.Create(null);
            Instance.blackOverlay.Show();
        }

        public static void Hide() {
            Instance.Window.Hide();
            Instance.blackOverlay.Hide();
        }

        public void ApplyRewards() {}
    }
}