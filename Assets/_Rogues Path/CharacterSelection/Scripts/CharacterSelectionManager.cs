using System;
using System.Collections.Generic;
using _Rogues_Path._Game;
using _Rogues_Path.Crafting;
using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.Utilities;
using DuloGames.UI;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Rogues_Path.CharacterSelection {
    public class CharacterSelectionManager : Singleton<CharacterSelectionManager> {
        [FoldoutGroup("Camera Properties"), SerializeField] private float CameraSpeed = 10f;
        [FoldoutGroup("Camera Properties"), SerializeField] private float CameraDistance = 10f;
        [FoldoutGroup("Camera Properties"), SerializeField] private Vector3 CameraDirection = Vector3.forward;

        [FoldoutGroup("References"), SerializeField] private Text NameText;
        [FoldoutGroup("References"), SerializeField] private Text ClassText;

        [FoldoutGroup("References"), SerializeField] private Button NextButton;

        private List<CharacterSelectionSlot> Slots = new List<CharacterSelectionSlot>();
        private int SelectedIndex = -1;
        private Transform SelectedTransform;

        private void Awake() {
            for (int i = 0; i < transform.childCount; i++) {
                Slots.Add(transform.GetChild(i).GetComponent<CharacterSelectionSlot>());
            }

            // Pick the middle-most slot
            SelectCharacter(Slots.GetRandomElement().GetComponent<CharacterSelectionSlot>());
        }

        protected void Update() {
            if (this.isActiveAndEnabled && this.Slots.Count == 0)
                return;

            // Make sure we have a slot transform
            if (this.SelectedTransform != null) {
                Vector3 targetPos = this.SelectedTransform.position + (this.CameraDirection * this.CameraDistance);
                targetPos.y = Camera.main!.transform.position.y;

                Camera.main!.transform.position = Vector3.Lerp(Camera.main!.transform.position, targetPos, Time.deltaTime * this.CameraSpeed);
            }
        }

        public void LockInCharacter() {
            NextButton.interactable = false;

            Game.Instance.PlayerData = Slots[SelectedIndex].PawnData;

            // Prepare spells
            Game.Instance.PlayerPreparedSpells.Clear();
            Game.Instance.PlayerPreparedSpells.AddRange(Game.Instance.PlayerData.ClassSpells);

            // New character = fresh authoritative equipment state.
            Game.Instance.PlayerEquipment.Clear();
            Game.Instance.PlayerInventory.Clear();
            Game.Instance.PlayerCurrentHealth = -1f;
            Game.Instance.WorldEncounterOrder.Clear();
            Game.Instance.CurrentWorldTileIndex = 0;

            foreach (EquipmentBase equipment in Slots[SelectedIndex].PawnData.StartingEquipment) {
                if (!EquipmentDatabase.TryGetID(equipment, out int equipmentID))
                    continue;

                Game.Instance.PlayerEquipment[equipment.EquipType] = new EquipmentInstanceData(equipmentID);
            }

            Game.FireTrigger(Trigger.EnterWorld);
        }

        public void SelectCharacter(CharacterSelectionSlot slot) {
            // Check if already selected
            if (this.SelectedIndex == slot.Index)
                return;

            // Set the selected
            this.SelectedIndex = slot.Index;
            this.SelectedTransform = slot.transform;

            // Update text
            NameText.text = slot.PawnData.Name;
            ClassText.text = slot.PawnData.ClassName;
        }

        public CharacterSelectionSlot GetCharacterInDirection(float direction) {
            if (this.Slots.Count == 0)
                return null;

            if (this.SelectedTransform == null && this.Slots[0] != null)
                return this.Slots[0].gameObject.GetComponent<CharacterSelectionSlot>();

            CharacterSelectionSlot closest = null;
            float lastDistance = 0f;

            foreach (CharacterSelectionSlot slot in this.Slots) {
                // Skip the selected one
                if (slot.Equals(this.SelectedTransform.GetComponent<CharacterSelectionSlot>()))
                    continue;

                float curDirection = slot.transform.position.x - this.SelectedTransform.position.x;

                // Check direction
                if (direction > 0f && curDirection > 0f || direction < 0f && curDirection < 0f) {

                    // Make sure we have slot component
                    if (slot == null)
                        continue;

                    // If we have no closest assigned yet
                    if (closest == null) {
                        closest = slot;
                        lastDistance = Vector3.Distance(this.SelectedTransform.position, slot.transform.position);
                        continue;
                    }

                    // Compare distance
                    if (Vector3.Distance(this.SelectedTransform.position, slot.transform.position) <= lastDistance) {
                        closest = slot;
                        lastDistance = Vector3.Distance(this.SelectedTransform.position, slot.transform.position);
                        continue;
                    }
                }
            }

            return closest;
        }

        public void SelectNext() {
            CharacterSelectionSlot next = this.GetCharacterInDirection(1f);

            if (next != null) {
                this.SelectCharacter(next);
            }
        }

        public void SelectPrevious() {
            CharacterSelectionSlot prev = this.GetCharacterInDirection(-1f);

            if (prev != null) {
                this.SelectCharacter(prev);
            }
        }
    }
}
