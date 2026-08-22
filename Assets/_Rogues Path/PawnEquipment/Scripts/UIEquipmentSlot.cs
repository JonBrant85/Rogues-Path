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
            return AssignInternal(equipment, null, instantiateEquipment: true);
        }

        public bool Assign(EquipmentBase equipment, Object source) {
            return AssignInternal(equipment, source, instantiateEquipment: false);
        }

        public bool Assign(object source) {
            if (source is not UIEquipmentSlot sourceSlot)
                return false;

            if (sourceSlot.Equipment == null)
                return false;

            /*
             * Static slots behave like templates.
             * Make an instance instead of stealing the static slot's object.
             */
            return AssignInternal(sourceSlot.Equipment, sourceSlot, instantiateEquipment: sourceSlot.isStatic);
        }

        private bool AssignInternal(EquipmentBase equipment, Object source, bool instantiateEquipment) {

            if (equipment == null) {
                Debug.LogError($"Attempting to assign null equipment to {EquipType}");

                return false;
            }

            if (!CheckEquipType(equipment))
                return false;

            UIEquipmentSlot sourceSlot = source as UIEquipmentSlot;

            /*
             * Don't allow silent cross-Pawn transfers.
             * They need their own explicit transaction because ownership,
             * inventory and modifiers all need moving between Pawns.
             */
            if (sourceSlot != null && sourceSlot.EquipToOwnerOnAssign && EquipToOwnerOnAssign && sourceSlot.Owner != null && Owner != null && sourceSlot.Owner != Owner) {

                Debug.LogWarning("Cross-Pawn equipment transfers are not supported " + "by UIEquipmentSlot.Assign.");

                return false;
            }

            EquipmentBase candidate = instantiateEquipment ? Instantiate(equipment, transform) : equipment;

            EquipmentBase previousUIEquipment = Equipment;

            /*
             * ------------------------------------------------------------
             * GAMEPLAY STATE FIRST
             * ------------------------------------------------------------
             *
             * Don't change the UI until we know the gameplay operation
             * succeeded.
             */

            EquipmentBase previouslyEquipped = null;
            bool candidateWasAlreadyEquipped = false;

            /*
             * Moving FROM an equipped slot TO a non-equipped UI slot.
             */
            if (!EquipToOwnerOnAssign && sourceSlot != null && sourceSlot.EquipToOwnerOnAssign && sourceSlot.Owner != null) {

                if (!sourceSlot.Owner.TryRemoveEquipment(candidate)) {
                    CleanupFailedCandidate(candidate, instantiateEquipment);

                    return false;
                }

                candidate.RemoveModifiers(candidate.Modifiers, sourceSlot.Owner);

                EventBus.Raise(new PawnStatChanged());
            }

            /*
             * Moving INTO an equipped slot.
             */
            if (EquipToOwnerOnAssign && Owner != null) {
                Owner.CurrentEquipment.TryGetValue(candidate.EquipType, out previouslyEquipped);

                candidateWasAlreadyEquipped = previouslyEquipped == candidate;

                if (!Owner.TryEquip(candidate)) {
                    CleanupFailedCandidate(candidate, instantiateEquipment);

                    return false;
                }

                /*
                 * Pawn.TryEquip handles equipment/inventory state.
                 *
                 * UIEquipmentSlot remains responsible for stat modifiers.
                 */
                if (!candidateWasAlreadyEquipped) {
                    if (previouslyEquipped != null && previouslyEquipped != candidate) {

                        previouslyEquipped.RemoveModifiers(previouslyEquipped.Modifiers, Owner);
                    }

                    candidate.ApplyModifiers(candidate.Modifiers, Owner);

                    EventBus.Raise(new PawnStatChanged());
                }
            }

            /*
             * ------------------------------------------------------------
             * GAMEPLAY SUCCEEDED — COMMIT UI
             * ------------------------------------------------------------
             */

            if (previousUIEquipment != null && previousUIEquipment != candidate) {

                OnUnassignEvent?.Invoke(Owner, previousUIEquipment);

                base.Unassign();

                /*
                 * Do NOT destroy it.
                 *
                 * Pawn.TryEquip may have just moved this exact runtime
                 * object into Inventory.
                 */
                if (previousUIEquipment.transform.parent == transform) {
                    previousUIEquipment.gameObject.SetActive(false);
                }
            }

            Equipment = candidate;

            Equipment.transform.SetParent(transform);
            Equipment.transform.localPosition = Vector3.zero;
            Equipment.gameObject.SetActive(true);

            base.Assign(Equipment.Icon);

            OnAssignEvent?.Invoke(Owner, Equipment);

            if (source != null) {
                OnAssignWithSourceEvent?.Invoke(this, source);
            }

            return true;
        }

        private void CleanupFailedCandidate(EquipmentBase candidate, bool instantiated) {

            if (!instantiated || candidate == null)
                return;

            Destroy(candidate.gameObject);
        }

        public override void Unassign() {
            if (Equipment == null)
                return;

            EquipmentBase equipment = Equipment;

            /*
             * Only alter Pawn equipment state if THIS runtime object is
             * actually the equipped object.
             *
             * A stale UI slot must never unequip some other sword just
             * because both are MeleeWeapon1H.
             */
            if (EquipToOwnerOnAssign && Owner != null) {
                bool actuallyEquipped = Owner.CurrentEquipment.TryGetValue(equipment.EquipType, out EquipmentBase equipped) && equipped == equipment;

                if (actuallyEquipped) {
                    if (!Owner.TryRemoveEquipment(equipment)) {
                        /*
                         * Usually this means inventory is full.
                         *
                         * Leave the UI alone because gameplay state wasn't
                         * changed.
                         */
                        return;
                    }

                    equipment.RemoveModifiers(equipment.Modifiers, Owner);

                    EventBus.Raise(new PawnStatChanged());
                }
            }

            base.Unassign();

            OnUnassignEvent?.Invoke(Owner, equipment);

            /*
             * Don't destroy the object here.
             *
             * TryRemoveEquipment() may have transferred the runtime
             * instance into the Pawn's inventory.
             */
            Equipment = null;
        }

        /*
         * Used after a successful drag transfer.
         *
         * Gameplay state was already handled by the destination slot, so
         * the source must ONLY forget its UI reference.
         */
        private void ClearTransferredReference() {
            if (Equipment == null)
                return;

            EquipmentBase oldEquipment = Equipment;

            base.Unassign();

            OnUnassignEvent?.Invoke(Owner, oldEquipment);

            Equipment = null;
        }

        public override bool IsAssigned() {
            return Equipment != null;
        }

        public override void OnPointerDown(PointerEventData eventData) {

            base.OnPointerDown(eventData);

            OnClickEvent.Invoke(this);
        }

        public virtual bool CheckEquipType(EquipmentBase equipment) {

            if (AcceptsAnyEquipType && equipment != null)
                return true;

            return equipment != null && equipment.EquipType == EquipType;
        }

        public bool PerformSlotSwap(Object sourceObject) {
            if (sourceObject is not UIEquipmentSlot targetSlot)
                return false;

            EquipmentBase myEquipment = Equipment;
            EquipmentBase theirEquipment = targetSlot.Equipment;

            if (myEquipment == null || theirEquipment == null) {

                return false;
            }

            if (!targetSlot.CheckEquipType(myEquipment) || !CheckEquipType(theirEquipment)) {

                return false;
            }

            /*
             * Cross-Pawn swapping deserves its own transaction.
             * Refuse it instead of silently corrupting two Pawns.
             */
            if (EquipToOwnerOnAssign && targetSlot.EquipToOwnerOnAssign && Owner != null && targetSlot.Owner != null && Owner != targetSlot.Owner) {

                Debug.LogWarning("Cross-Pawn equipment swapping is not supported.");

                return false;
            }

            /*
             * Same-owner equipped-slot -> equipped-slot:
             *
             * Both items are already equipped. Their EquipmentPart doesn't
             * change just because their visual slots swap, so gameplay
             * state and modifiers don't need to change.
             */
            if (EquipToOwnerOnAssign && targetSlot.EquipToOwnerOnAssign && Owner == targetSlot.Owner) {

                SetEquipmentUIOnly(theirEquipment, targetSlot);

                targetSlot.SetEquipmentUIOnly(myEquipment, this);

                return true;
            }

            /*
             * Equipped slot -> inventory-like slot.
             *
             * Equip their item. Pawn.TryEquip atomically moves my item into
             * inventory and removes their item from inventory.
             */
            if (EquipToOwnerOnAssign && !targetSlot.EquipToOwnerOnAssign && Owner != null) {

                if (!Owner.TryEquip(theirEquipment))
                    return false;

                myEquipment.RemoveModifiers(myEquipment.Modifiers, Owner);

                theirEquipment.ApplyModifiers(theirEquipment.Modifiers, Owner);

                EventBus.Raise(new PawnStatChanged());
            }
            /*
             * Inventory-like slot -> equipped slot.
             */
            else if (!EquipToOwnerOnAssign && targetSlot.EquipToOwnerOnAssign && targetSlot.Owner != null) {

                if (!targetSlot.Owner.TryEquip(myEquipment))
                    return false;

                theirEquipment.RemoveModifiers(theirEquipment.Modifiers, targetSlot.Owner);

                myEquipment.ApplyModifiers(myEquipment.Modifiers, targetSlot.Owner);

                EventBus.Raise(new PawnStatChanged());
            }

            SetEquipmentUIOnly(theirEquipment, targetSlot);

            targetSlot.SetEquipmentUIOnly(myEquipment, this);

            return true;
        }

        private void SetEquipmentUIOnly(EquipmentBase equipment, Object source) {

            EquipmentBase previous = Equipment;

            if (previous != null) {
                OnUnassignEvent?.Invoke(Owner, previous);
            }

            base.Unassign();

            Equipment = equipment;

            if (Equipment == null)
                return;

            Equipment.transform.SetParent(transform);
            Equipment.transform.localPosition = Vector3.zero;
            Equipment.gameObject.SetActive(true);

            base.Assign(Equipment.Icon);

            OnAssignEvent?.Invoke(Owner, Equipment);

            if (source != null) {
                OnAssignWithSourceEvent?.Invoke(this, source);
            }
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

        public override void OnDrop(PointerEventData eventData) {

            UIEquipmentSlot source = eventData.pointerPress != null ? eventData.pointerPress.GetComponent<UIEquipmentSlot>() : null;

            if (source == null || !source.IsAssigned() || !source.dragAndDropEnabled) {

                return;
            }

            source.dropPreformed = true;

            if (!enabled || !m_DragAndDropEnabled)
                return;

            bool assignSuccess = false;

            /*
             * Empty target.
             */
            if (!IsAssigned()) {
                assignSuccess = Assign(source);

                if (assignSuccess && !source.isStatic) {
                    /*
                     * Do NOT call source.Unassign().
                     *
                     * The destination already handled the gameplay
                     * transaction. Calling Unassign here can unequip the
                     * item we JUST equipped.
                     */
                    source.ClearTransferredReference();
                }
            }
            /*
             * Occupied target.
             */
            else {
                if (!isStatic && !source.isStatic) {
                    if (CanSwapWith(source) && source.CanSwapWith(this)) {

                        assignSuccess = source.PerformSlotSwap(this);
                    }
                }
                else if (!isStatic && source.isStatic) {
                    /*
                     * Assign() detects a static source and clones it.
                     */
                    assignSuccess = Assign(source);
                }
            }

            if (!assignSuccess) {
                OnAssignBySlotFailed(source);
            }
        }

        public override void OnTooltip(bool show) {
            UITooltip.InstantiateIfNecessary(gameObject);

            if (IsAssigned()) {
                if (show) {
                    PrepareTooltip(Equipment);

                    UITooltip.AnchorToRect(transform as RectTransform);

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

                    UITooltip.AnchorToRect(transform as RectTransform);

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

            if (UITooltipManager.Instance != null) {
                UITooltip.SetWidth(UITooltipManager.Instance.itemTooltipWidth);
            }

            UITooltip.AddTitle("<color=#" + UIItemQualityColor.GetHexColor(equipment.Quality) + ">" + equipment.Name + "</color>");

            UITooltip.AddSpacer();

            UITooltip.AddLineColumn(EquipTypeToString(equipment.EquipType), "ItemAttribute");

            UITooltip.AddLineColumn(equipment.Quality.ToString());

            if (equipment.Modifiers != null) {
                foreach (StatAndModifierPair pair in equipment.Modifiers) {

                    UITooltip.AddLineColumn(pair.StatID.name, "ItemAttribute");

                    UITooltip.AddLineColumn(pair.Modifier.Value.ToString("N0"), "ItemAttribute");
                }
            }

            if (!string.IsNullOrEmpty(equipment.Description)) {

                UITooltip.AddSpacer();

                UITooltip.AddLine(equipment.Description, "ItemAttribute");
            }

            if (!string.IsNullOrEmpty(equipment.FlavorText)) {

                UITooltip.AddSpacer();

                UITooltip.AddLine(equipment.FlavorText, "ItemDescription");
            }
        }

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