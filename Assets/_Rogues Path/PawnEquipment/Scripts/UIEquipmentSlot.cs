using System;
using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.Pawns;
using _Rogues_Path.Pawns.Scripts;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using DuloGames.UI;
using HeroEditor.Common.Enums;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using EquipmentPart = Assets.HeroEditor4D.Common.Scripts.Enums.EquipmentPart;
using Object = System.Object;

namespace _Rogues_Path.UI.Slots {
    public class OnAssign : UnityEvent<Pawn, EquipmentBase> {}

    [Serializable] public class OnAssignWithSource : UnityEvent<UIEquipmentSlot, Object> {}

    [Serializable] public class OnEquipmentSlotClickEvent : UnityEvent<UIEquipmentSlot> {}

    public class UIEquipmentSlot : UISlotBase {
        public bool EquipToOwnerOnAssign = false;
        public OnEquipmentSlotClickEvent OnClickEvent = new();
        public OnAssign OnAssignEvent = new();
        public OnAssign OnUnassignEvent = new();
        public OnAssignWithSource OnAssignWithSourceEvent = new();
        public bool AcceptsAnyEquipType = false;
        public EquipmentPart EquipType;
        public Pawn Owner;
        public EquipmentBase Equipment;

        public bool Assign(EquipmentBase equipment) {
            if (equipment == null) {
                Debug.LogError($"Attempting to assign null equipment to {EquipType}");
                return false;
            }

            if (!CheckEquipType(equipment)) return false;

            Unassign();

            // Instantiate and OWN the instance
            Equipment = Instantiate(equipment, transform);
            Equipment.transform.localPosition = Vector3.zero;
            Equipment.gameObject.SetActive(true);

            Debug.Assert(Equipment != null, "Equipment instantiation failed");

            // Icon
            base.Assign(Equipment.Icon);

            // Events
            OnAssignEvent?.Invoke(Owner, Equipment);

            if (EquipToOwnerOnAssign && Owner != null) {
                Debug.Assert(Equipment != null);
                Owner.TryEquip(Equipment);
            }

            return true;
        }

        public bool Assign(EquipmentBase equipment, Object source) {
            if (equipment == null)
                return false;

            if (!CheckEquipType(equipment))
                return false;

            base.Unassign();

            Equipment = equipment;
            Equipment.transform.SetParent(transform);
            Equipment.transform.localPosition = Vector3.zero;
            Equipment.gameObject.SetActive(true);

            base.Assign(Equipment.Icon);

            OnAssignEvent?.Invoke(Owner, Equipment);
            OnAssignWithSourceEvent?.Invoke(this, source);

            if (EquipToOwnerOnAssign && Owner != null) {
                Debug.Assert(Equipment != null);

                if (Owner.TryEquip(Equipment)) {
                    Equipment.ApplyModifiers(Equipment.Modifiers, Owner);
                    EventBus.Raise(new PawnStatChanged());
                }
            }

            return true;
        }

        public bool Assign(object source) {
            if (source is UIEquipmentSlot sourceSlot) {
                if (sourceSlot.Equipment == null)
                    return false;

                return Assign(sourceSlot.Equipment, sourceSlot);
            }

            return false;
        }

        public override void Unassign() {
            if (Equipment == null)
                return;

            base.Unassign();



            OnUnassignEvent?.Invoke(Owner, Equipment);

            // Only destroy if THIS slot owns it
            if (Equipment.transform.parent == transform) {
                //Destroy(Equipment.gameObject);
            }

            if (EquipToOwnerOnAssign && Owner != null) {
                Owner.TryRemoveEquipment(Equipment);
                Equipment.RemoveModifiers(Equipment.Modifiers, Owner);
                EventBus.Raise(new PawnStatChanged());
            }

            Equipment = null;
        }

        public override bool IsAssigned() => Equipment != null;

        public override void OnPointerDown(PointerEventData eventData) {
            base.OnPointerDown(eventData);
            OnClickEvent.Invoke(this);
        }

        public virtual bool CheckEquipType(EquipmentBase equipment) {
            if (AcceptsAnyEquipType && equipment != null) return true;
            return equipment != null && equipment.EquipType == EquipType;
        }

        public bool PerformSlotSwap(Object sourceObject) {
            if (sourceObject is not UIEquipmentSlot sourceSlot)
                return false;

            EquipmentBase myEquip = Equipment;
            EquipmentBase theirEquip = sourceSlot.Equipment;

            // Temporarily suppress Unassign side effects
            Debug.Assert(Owner != null);
            Equipment.RemoveModifiers(Equipment.Modifiers, Owner);
            Equipment = null;
            sourceSlot.Equipment = null;

            bool a = sourceSlot.Assign(myEquip, this);
            bool b = Assign(theirEquip, sourceSlot);
            Equipment.ApplyModifiers(Equipment.Modifiers, Owner);
            return a && b;
        }

        public bool CanSwapWith(Object target) {
            return target switch {
                UIEquipmentSlot slot => slot.CheckEquipType(Equipment),
                UIItemSlot => true,
                _ => false
            };
        }

        public EquipmentBase GetEquipmentInfo() {
            return Equipment;
        }

        /// <summary>
        /// Raises the drop event.
        /// </summary>
        /// <param name="eventData">Event data.</param>
        public override void OnDrop(PointerEventData eventData) {
            // Get the source slot
            UIEquipmentSlot source = (eventData.pointerPress != null) ? eventData.pointerPress.GetComponent<UIEquipmentSlot>() : null;

            // Make sure we have the source slot
            if (source == null || !source.IsAssigned() || !source.dragAndDropEnabled)
                return;

            // Notify the source that a drop was performed so it does not unassign
            source.dropPreformed = true;

            // Check if this slot is enabled and it's drag and drop feature is enabled
            if (!this.enabled || !this.m_DragAndDropEnabled)
                return;

            // Prepare a variable indicating whether the assign process was successful
            bool assignSuccess = false;

            // Normal empty slot assignment
            if (!this.IsAssigned()) {
                // Assign the target slot with the info from the source
                assignSuccess = this.Assign(source);

                // Unassign the source on successful assignment and the source is not static
                if (assignSuccess && !source.isStatic)
                    source.Unassign();
            }
            // The target slot is assigned
            else {
                // If the target slot is not static
                // and we have a source slot that is not static
                if (!this.isStatic && !source.isStatic) {
                    // Check if we can swap
                    if (this.CanSwapWith(source) && source.CanSwapWith(this)) {
                        // Swap the slots
                        assignSuccess = source.PerformSlotSwap(this);
                    }
                }
                // If the target slot is not static
                // and the source slot is a static one
                else if (!this.isStatic && source.isStatic) {
                    assignSuccess = this.Assign(source);
                }
            }

            // If this slot failed to be assigned
            if (!assignSuccess) {
                this.OnAssignBySlotFailed(source);
            }
        }

        public override void OnTooltip(bool show) {
            UITooltip.InstantiateIfNecessary(this.gameObject);

            if (IsAssigned()) {
                if (show) {
                    PrepareTooltip(Equipment);
                    UITooltip.AnchorToRect(this.transform as RectTransform);
                    UITooltip.Show();
                }
                else {
                    UITooltip.Hide();
                }
            }
            else {
                if (show) {
                    UITooltip.AddTitle(EquipType.ToString());
                    UITooltip.SetHorizontalFitMode(ContentSizeFitter.FitMode.PreferredSize);
                    UITooltip.AnchorToRect(this.transform as RectTransform);
                    UITooltip.Show();
                }
                else {
                    UITooltip.Hide();
                }
            }
        }

        #region Static Methods
        public static void PrepareTooltip(EquipmentBase equipment) {
            if (equipment == null)
                return;

            // Set the tooltip width
            if (UITooltipManager.Instance != null)
                UITooltip.SetWidth(UITooltipManager.Instance.itemTooltipWidth);

            // Set the title and description
            UITooltip.AddTitle("<color=#" + UIItemQualityColor.GetHexColor(equipment.Quality) + ">" + equipment.Name + "</color>");

            // Spacer
            UITooltip.AddSpacer();

            // Item types
            UITooltip.AddLineColumn(EquipTypeToString(equipment.EquipType), "ItemAttribute");
            UITooltip.AddLineColumn(equipment.Quality.ToString());

            foreach (StatAndModifierPair pair in equipment.Modifiers) {
                // pair.Modifier.Value
                // pair.StatID.name
                UITooltip.AddLineColumn(pair.StatID.name, "ItemAttribute");
                UITooltip.AddLineColumn(pair.Modifier.Value.ToString("N0"), "ItemAttribute");
            }

            // Set the item description if not empty
            if (!string.IsNullOrEmpty(equipment.Description)) {
                UITooltip.AddSpacer();
                UITooltip.AddLine(equipment.Description, "ItemAttribute");
            }

            // Set the flavor text if not empty
            if (!string.IsNullOrEmpty(equipment.FlavorText)) {
                UITooltip.AddSpacer();
                UITooltip.AddLine(equipment.FlavorText, "ItemDescription");
            }
        }

        /// <summary>
        /// Equipment part to string conversion.
        /// </summary>
        /// <returns>The string.</returns>
        public static string EquipTypeToString(Assets.HeroEditor4D.Common.Scripts.Enums.EquipmentPart type) {
            return type switch {
                EquipmentPart.Armor => "Armor",
                EquipmentPart.Helmet => "Helmet",
                EquipmentPart.Vest => "Vest",
                EquipmentPart.Bracers => "Bracers",
                EquipmentPart.Leggings => "Leggings",
                EquipmentPart.MeleeWeapon1H => "Melee Weapon 1H",
                EquipmentPart.MeleeWeapon2H => "Melee Weapon 2H",
                EquipmentPart.Bow => "Bow",
                EquipmentPart.Crossbow => "Crossbow",
                EquipmentPart.SecondaryMelee1H => "Secondary Melee 1H",
                EquipmentPart.SecondaryFirearm1H => "SecondaryFirearm1H",
                EquipmentPart.Shield => "Shield",
                EquipmentPart.Earrings => "Earrings",
                EquipmentPart.Cape => "Cape",
                EquipmentPart.Quiver => "Quiver",
                EquipmentPart.Back => "Back",
                EquipmentPart.Mask => "Mask",
                EquipmentPart.Firearm1H => "Firearm 1H",
                EquipmentPart.Firearm2H => "Firearm 2H",
                EquipmentPart.Wings => "Wings",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
        #endregion
    }
}