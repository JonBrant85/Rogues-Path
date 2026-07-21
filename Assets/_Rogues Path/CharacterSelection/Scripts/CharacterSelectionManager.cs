using System;
using System.Collections.Generic;
using _Rogues_Path._Game;
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

        private List<Transform> Slots = new List<Transform>();
        private int SelectedIndex = -1;
        private Transform SelectedTransform;

        private void Awake() {
            for (int i = 0; i < transform.childCount; i++) {
                Slots.Add(transform.GetChild(i));
            }
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

        public void SelectCharacter(CharacterSelectionSlot slot) {
            // Check if already selected
            if (this.SelectedIndex == slot.Index)
                return;

            // Deselect
            if (this.SelectedIndex > -1) {
                // Get the slot
                Transform selectedSlotTrans = this.Slots[this.SelectedIndex];
            }

            // Set the selected
            this.SelectedIndex = slot.Index;
            this.SelectedTransform = slot.transform;

            // Update text
            NameText.text = slot.Pawn.CharacterName;
            ClassText.text = slot.Pawn.ClassName;

            Game.Instance.CurrentCharacter = slot.Pawn;
        }

        public CharacterSelectionSlot GetCharacterInDirection(float direction) {
            if (this.Slots.Count == 0)
                return null;

            if (this.SelectedTransform == null && this.Slots[0] != null)
                return this.Slots[0]
                    .gameObject.GetComponent<CharacterSelectionSlot>();

            CharacterSelectionSlot closest = null;
            float lastDistance = 0f;

            foreach (Transform trans in this.Slots) {
                // Skip the selected one
                if (trans.Equals(this.SelectedTransform))
                    continue;

                float curDirection = trans.position.x - this.SelectedTransform.position.x;

                // Check direction
                if (direction > 0f && curDirection > 0f || direction < 0f && curDirection < 0f) {
                    // Get the character component
                    CharacterSelectionSlot slot = trans.GetComponent<CharacterSelectionSlot>();

                    // Make sure we have slot component
                    if (slot == null)
                        continue;

                    // If we have no closest assigned yet
                    if (closest == null) {
                        closest = slot;
                        lastDistance = Vector3.Distance(this.SelectedTransform.position, trans.position);
                        continue;
                    }

                    // Compare distance
                    if (Vector3.Distance(this.SelectedTransform.position, trans.position) <= lastDistance) {
                        closest = slot;
                        lastDistance = Vector3.Distance(this.SelectedTransform.position, trans.position);
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