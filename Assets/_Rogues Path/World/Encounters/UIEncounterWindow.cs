using System.Collections.Generic;
using _Rogues_Path.Crafting;
using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.PawnEquipment.Scripts;
using _Rogues_Path.UI;
using _Rogues_Path.UI.Slots;
using _Rogues_Path.Utilities;
using Cysharp.Threading.Tasks;
using DuloGames.UI;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Rogues_Path.World.Encounters {
    public class UIEncounterWindow : Singleton<UIEncounterWindow> {
        [FoldoutGroup("References"), SerializeField] private Text titleText;
        [FoldoutGroup("References"), SerializeField] private Text bodyText;
        [FoldoutGroup("References"), SerializeField] private UIPositionMover BottomBarMover;
        [FoldoutGroup("References"), SerializeField] private UIWindow Window;
        [FoldoutGroup("References"), SerializeField] private Transform WindowContent;
        [FoldoutGroup("References"), SerializeField] private Transform BottomBar;
        [FoldoutGroup("References"), SerializeField] private Button ButtonPrefab;
        [FoldoutGroup("References"), SerializeField] private Transform EquipmentChoicesContainer;
        [FoldoutGroup("References"), SerializeField] private UIEquipmentSlot EquipmentSlotPrefab;

        [FoldoutGroup("Settings"), SerializeField] private Vector2 EquipmentChoiceSlotSize = new(96f, 96f);
        [FoldoutGroup("Settings"), SerializeField] private Color EquipmentChoiceHighlightColor = new(1f, 0.72f, 0.2f, 1f);

        private readonly List<UIEquipmentSlot> equipmentChoiceSlots = new();
        private readonly Dictionary<UIEquipmentSlot, Outline> equipmentChoiceOutlines = new();
        private UIEquipmentSlot selectedEquipmentChoice;

        public async UniTask LoadEncounter(EncounterData data) {
            if (data == null) {
                Debug.LogError("Cannot load a null encounter.");
                return;
            }

            if (!data.UsesEncounterWindow) {
                await data.HandleEncounter(WindowContent, BottomBar, ButtonPrefab);
                return;
            }

            titleText.text = data.EncounterTitle;

            if (bodyText != null)
                bodyText.text = data.EncounterDescription;

            Show();
            await data.HandleEncounter(WindowContent, BottomBar, ButtonPrefab);

            if (this == null)
                return;

            Hide();
        }

        public async UniTask<EquipmentInstanceData> WaitForEquipmentSelection(
            IReadOnlyList<EquipmentBase> equipmentChoices,
            string buttonText) {

            ClearEquipmentChoices();

            if (equipmentChoices == null || equipmentChoices.Count == 0) {
                Debug.LogError("Cannot display an empty equipment selection.");
                return null;
            }

            if (EquipmentChoicesContainer == null || EquipmentSlotPrefab == null || BottomBar == null || ButtonPrefab == null) {
                Debug.LogError("Equipment selection UI is not configured.");
                return null;
            }

            EquipmentChoicesContainer.gameObject.SetActive(true);

            Button selectionButton = Instantiate(ButtonPrefab, BottomBar);
            Text label = selectionButton.GetComponentInChildren<Text>();

            if (label != null)
                label.text = buttonText;

            selectionButton.interactable = false;
            selectionButton.gameObject.SetActive(true);

            foreach (EquipmentBase equipment in equipmentChoices) {
                if (equipment == null || !EquipmentDatabase.TryGetID(equipment, out int equipmentID))
                    continue;

                UIEquipmentSlot slot = Instantiate(EquipmentSlotPrefab, EquipmentChoicesContainer);
                slot.name = $"Equipment Choice ({equipment.Name})";
                slot.EquipToOwnerOnAssign = false;
                slot.AcceptedEquipTypes = EquipmentPartMask.All;
                slot.dragAndDropEnabled = false;
                slot.isStatic = true;
                slot.allowThrowAway = false;

                if (slot.transform is RectTransform slotRect)
                    slotRect.sizeDelta = EquipmentChoiceSlotSize;

                if (!slot.Assign(equipment)) {
                    Destroy(slot.gameObject);
                    continue;
                }

                slot.BindInstanceData(new EquipmentInstanceData(equipmentID), null);
                slot.OnClickEvent.AddListener(SelectEquipment);

                Outline outline = slot.gameObject.AddComponent<Outline>();
                outline.effectColor = EquipmentChoiceHighlightColor;
                outline.effectDistance = new Vector2(4f, -4f);
                outline.useGraphicAlpha = false;
                outline.enabled = false;

                equipmentChoiceSlots.Add(slot);
                equipmentChoiceOutlines.Add(slot, outline);
                slot.gameObject.SetActive(true);
            }

            if (equipmentChoiceSlots.Count == 0) {
                Debug.LogError("No valid equipment choices could be displayed.");
                Destroy(selectionButton.gameObject);
                ClearEquipmentChoices();
                return null;
            }

            bool confirmed = false;
            selectionButton.onClick.AddListener(ConfirmSelection);

            await UniTask.WaitUntil(() => confirmed || this == null);

            if (this == null)
                return null;

            EquipmentInstanceData selectedInstanceData = selectedEquipmentChoice?.InstanceData;

            selectionButton.onClick.RemoveListener(ConfirmSelection);
            Destroy(selectionButton.gameObject);
            ClearEquipmentChoices();

            return selectedInstanceData;

            void SelectEquipment(UIEquipmentSlot selectedSlot) {
                selectedEquipmentChoice = selectedSlot;
                selectionButton.interactable = true;

                foreach (KeyValuePair<UIEquipmentSlot, Outline> choice in equipmentChoiceOutlines)
                    choice.Value.enabled = choice.Key == selectedEquipmentChoice;
            }

            void ConfirmSelection() {
                if (selectedEquipmentChoice == null)
                    return;

                selectionButton.interactable = false;
                confirmed = true;
            }
        }

        public void Show() {
            Window.Show();
            BottomBarMover.Hide();
        }

        public void Hide() {
            Window.Hide();
            BottomBarMover.Show();
        }

        private void ClearEquipmentChoices() {
            foreach (UIEquipmentSlot slot in equipmentChoiceSlots) {
                if (slot == null)
                    continue;

                slot.OnClickEvent.RemoveAllListeners();
                Destroy(slot.gameObject);
            }

            equipmentChoiceSlots.Clear();
            equipmentChoiceOutlines.Clear();
            selectedEquipmentChoice = null;

            if (EquipmentChoicesContainer != null)
                EquipmentChoicesContainer.gameObject.SetActive(false);
        }
    }
}
