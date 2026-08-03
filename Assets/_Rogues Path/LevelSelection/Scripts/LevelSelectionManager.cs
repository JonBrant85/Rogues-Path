using System.Collections.Generic;
using _Rogues_Path._Game;
using _Rogues_Path.CharacterSelection;
using _Rogues_Path.Utilities;
using DuloGames.UI;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Rogues_Path.LevelSelection {
    public class LevelSelectionManager : Singleton<LevelSelectionManager> {
        [FoldoutGroup("Camera Properties"), SerializeField] private float CameraSpeed = 10f;
        [FoldoutGroup("Camera Properties"), SerializeField] private float CameraDistance = 10f;
        [FoldoutGroup("Camera Properties"), SerializeField] private Vector3 CameraDirection = Vector3.forward;

        [FoldoutGroup("References"), SerializeField] private Text NameText;
        [FoldoutGroup("References"), SerializeField] private Text DescriptionText;

        private List<Transform> Slots = new List<Transform>();
        private int SelectedIndex = -1;
        private Transform SelectedTransform;

        private void Awake() {
            for (int i = 0; i < transform.childCount; i++) {
                Slots.Add(transform.GetChild(i));
            }
            
            // Pick the middle-most slot
            SelectLevel(Slots.GetRandomElement().GetComponent<LevelSelectionSlot>());
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

        public void SelectLevel(LevelSelectionSlot slot) {
            // Check if already selected
            if (this.SelectedIndex == slot.Index)
                return;

            // Set the selected
            this.SelectedIndex = slot.Index;
            this.SelectedTransform = slot.transform;

            // Update text
            NameText.text = slot.LevelData.LevelName;
            DescriptionText.text = slot.LevelData.Description;

            Game.Instance.LevelData = slot.LevelData;
        }

        public LevelSelectionSlot GetCharacterInDirection(float direction) {
            if (this.Slots.Count == 0)
                return null;

            if (this.SelectedTransform == null && this.Slots[0] != null)
                return this.Slots[0]
                    .gameObject.GetComponent<LevelSelectionSlot>();

            LevelSelectionSlot closest = null;
            float lastDistance = 0f;

            foreach (Transform trans in this.Slots) {
                // Skip the selected one
                if (trans.Equals(this.SelectedTransform))
                    continue;

                float curDirection = trans.position.x - this.SelectedTransform.position.x;

                // Check direction
                if (direction > 0f && curDirection > 0f || direction < 0f && curDirection < 0f) {
                    // Get the character component
                    LevelSelectionSlot slot = trans.GetComponent<LevelSelectionSlot>();

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
            LevelSelectionSlot next = this.GetCharacterInDirection(1f);

            if (next != null) {
                this.SelectLevel(next);
            }
        }

        public void SelectPrevious() {
            LevelSelectionSlot prev = this.GetCharacterInDirection(-1f);

            if (prev != null) {
                this.SelectLevel(prev);
            }
        }
    }
}