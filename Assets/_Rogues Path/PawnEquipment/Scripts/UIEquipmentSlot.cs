using System;
using System.Linq;
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


        #region Assign
        public bool Assign(EquipmentBase equipment) {
            return AssignDirect(equipment, null);
        }


        public bool Assign(EquipmentBase equipment, Object source) {

            if (source is UIEquipmentSlot sourceSlot) {
                return AssignFromSlot(sourceSlot);
            }

            return AssignDirect(equipment, source);
        }


        public bool Assign(object source) {
            if (source is not UIEquipmentSlot sourceSlot)
                return false;

            if (sourceSlot.Equipment == null)
                return false;

            return AssignFromSlot(sourceSlot);
        }


        private bool AssignDirect(EquipmentBase equipment, Object source) {

            if (equipment == null) {
                Debug.LogError($"Attempting to assign null equipment to {EquipType}.");

                return false;
            }

            if (!CheckEquipType(equipment))
                return false;


            /*
             * =========================================================
             * EQUIPPED SLOT
             * =========================================================
             */

            if (EquipToOwnerOnAssign) {
                if (Owner == null) {
                    Debug.LogError($"{name} is configured as an equipment slot " + $"but has no Owner.");

                    return false;
                }

                /*
                 * DATABASE TEMPLATE
                 *
                 * Create a fresh live object and ask the Pawn to perform
                 * the actual gameplay transaction.
                 */
                if (EquipmentDatabase.IsDatabaseEntry(equipment)) {
                    if (!EquipmentDatabase.TryCreateInstance(equipment, out EquipmentBase liveEquipment, transform)) {

                        return false;
                    }

                    if (!Owner.TryEquip(liveEquipment, true)) {

                        Destroy(liveEquipment.gameObject);

                        return false;
                    }

                    BindLiveReference(liveEquipment, source);

                    return true;
                }


                /*
                 * LIVE INSTANCE
                 *
                 * If the Pawn already owns this exact live object,
                 * we're merely attaching UI to existing runtime state.
                 *
                 * This is useful when UI is instantiated AFTER the Pawn.
                 */
                if (Owner.CurrentEquipment.TryGetValue(equipment.EquipType, out EquipmentBase current) && current == equipment) {

                    BindLiveReference(equipment, source);

                    return true;
                }


                /*
                 * Otherwise this is a new live instance supplied by
                 * something other than the database factory call above.
                 */
                if (!Owner.TryEquip(equipment, true)) {

                    return false;
                }

                BindLiveReference(equipment, source);

                return true;
            }


            /*
             * =========================================================
             * INVENTORY / NON-EQUIPPED SLOT
             * =========================================================
             *
             * Never keep the live object here.
             *
             * Always display its database/template representation.
             */

            if (!EquipmentDatabase.TryFind(equipment, out EquipmentBase dbEquipment)) {

                return false;
            }

            BindDatabaseReference(dbEquipment, source);

            return true;
        }


        private bool AssignFromSlot(UIEquipmentSlot sourceSlot) {

            if (sourceSlot == null || sourceSlot.Equipment == null) {

                return false;
            }

            if (sourceSlot == this)
                return true;

            if (!CheckEquipType(sourceSlot.Equipment)) {

                return false;
            }


            /*
             * Static slots are templates/sources.
             *
             * Copy from them; never remove their reference.
             */
            if (sourceSlot.isStatic) {
                return AssignDirect(sourceSlot.Equipment, sourceSlot);
            }


            /*
             * An occupied target means this is really a swap.
             */
            if (IsAssigned()) {
                return sourceSlot.PerformSlotSwap(this);
            }


            bool sourceEquipped = sourceSlot.EquipToOwnerOnAssign;

            bool targetEquipped = EquipToOwnerOnAssign;


            /*
             * =========================================================
             * INVENTORY -> INVENTORY
             * =========================================================
             */

            if (!sourceEquipped && !targetEquipped) {

                if (!EquipmentDatabase.TryFind(sourceSlot.Equipment, out EquipmentBase dbEquipment)) {

                    return false;
                }

                BindDatabaseReference(dbEquipment, sourceSlot);

                sourceSlot.ClearReferenceOnly();

                return true;
            }


            /*
             * =========================================================
             * INVENTORY -> EQUIPPED
             * =========================================================
             */

            if (!sourceEquipped && targetEquipped) {

                if (Owner == null)
                    return false;

                if (!EquipmentDatabase.TryCreateInstance(sourceSlot.Equipment, out EquipmentBase liveEquipment, transform)) {

                    return false;
                }

                if (!Owner.TryEquip(liveEquipment, true)) {

                    Destroy(liveEquipment.gameObject);

                    return false;
                }

                BindLiveReference(liveEquipment, sourceSlot);

                sourceSlot.ClearReferenceOnly();

                return true;
            }


            /*
             * =========================================================
             * EQUIPPED -> INVENTORY
             * =========================================================
             */

            if (sourceEquipped && !targetEquipped) {

                if (sourceSlot.Owner == null)
                    return false;

                EquipmentBase liveEquipment = sourceSlot.Equipment;

                if (!EquipmentDatabase.TryFind(liveEquipment, out EquipmentBase dbEquipment)) {

                    return false;
                }

                if (!sourceSlot.Owner.TryRemoveEquipment(liveEquipment, true)) {

                    return false;
                }

                /*
                 * Gameplay succeeded.
                 *
                 * Pawn destroyed the live object and added the DB ID
                 * back into Game.PlayerInventory.
                 */
                BindDatabaseReference(dbEquipment, sourceSlot);

                sourceSlot.ClearReferenceOnly();

                return true;
            }


            /*
             * =========================================================
             * EQUIPPED -> EQUIPPED
             * =========================================================
             *
             * No gameplay state changes.
             *
             * The live object is still equipped with the same EquipType.
             * We're just moving which UI slot displays it.
             */

            if (sourceEquipped && targetEquipped) {

                if (sourceSlot.Owner != Owner)
                    return false;

                EquipmentBase liveEquipment = sourceSlot.Equipment;

                BindLiveReference(liveEquipment, sourceSlot);

                sourceSlot.ClearReferenceOnly();

                return true;
            }


            return false;
        }
        #endregion


        #region Unassign
        public override void Unassign() {
            if (Equipment == null)
                return;

            EquipmentBase oldEquipment = Equipment;


            /*
             * EQUIPPED SLOT
             */
            if (EquipToOwnerOnAssign && Owner != null && !EquipmentDatabase.IsDatabaseEntry(oldEquipment)) {

                bool actuallyEquipped = Owner.CurrentEquipment.TryGetValue(oldEquipment.EquipType, out EquipmentBase current) && current == oldEquipment;

                if (actuallyEquipped) {
                    if (!Owner.TryRemoveEquipment(oldEquipment, true)) {

                        /*
                         * Do not change UI if gameplay refused
                         * the unequip.
                         */
                        return;
                    }
                }
            }


            base.Unassign();

            OnUnassignEvent?.Invoke(Owner, oldEquipment);

            Equipment = null;
        }


        private void ClearReferenceOnly() {
            if (Equipment == null)
                return;

            EquipmentBase oldEquipment = Equipment;

            base.Unassign();

            OnUnassignEvent?.Invoke(Owner, oldEquipment);

            Equipment = null;
        }
        #endregion


        #region Reference Binding
        private void BindDatabaseReference(EquipmentBase dbEquipment, Object source) {

            EquipmentBase previous = Equipment;

            if (previous != null && previous != dbEquipment) {

                base.Unassign();

                OnUnassignEvent?.Invoke(Owner, previous);
            }

            /*
             * NEVER parent or activate database equipment.
             */
            Equipment = dbEquipment;

            base.Assign(Equipment.Icon);

            OnAssignEvent?.Invoke(Owner, Equipment);

            if (source != null) {
                OnAssignWithSourceEvent?.Invoke(this, source);
            }
        }


        private void BindLiveReference(EquipmentBase liveEquipment, Object source) {

            EquipmentBase previous = Equipment;

            if (previous != null && previous != liveEquipment) {

                base.Unassign();

                OnUnassignEvent?.Invoke(Owner, previous);
            }

            Equipment = liveEquipment;

            Equipment.transform.SetParent(transform, false);

            Equipment.transform.localPosition = Vector3.zero;

            /*
             * Pawn controls activation.
             * Don't toggle it here.
             */

            base.Assign(Equipment.Icon);

            OnAssignEvent?.Invoke(Owner, Equipment);

            if (source != null) {
                OnAssignWithSourceEvent?.Invoke(this, source);
            }
        }
        #endregion


        public override bool IsAssigned() =>
            Equipment != null;


        public override void OnPointerDown(PointerEventData eventData) {

            base.OnPointerDown(eventData);

            OnClickEvent.Invoke(this);
        }


        public virtual bool CheckEquipType(EquipmentBase equipment) {

            if (AcceptsAnyEquipType && equipment != null) {

                return true;
            }

            return equipment != null && equipment.EquipType == EquipType;
        }


        #region Swap
        public bool PerformSlotSwap(Object sourceObject) {

            if (sourceObject is not UIEquipmentSlot otherSlot)
                return false;

            if (Equipment == null || otherSlot.Equipment == null) {

                return false;
            }

            /*
             * Each item must be legal in its destination.
             */
            if (!otherSlot.CheckEquipType(Equipment) || !CheckEquipType(otherSlot.Equipment)) {

                return false;
            }


            bool thisEquipped = EquipToOwnerOnAssign;

            bool otherEquipped = otherSlot.EquipToOwnerOnAssign;


            /*
             * =========================================================
             * INVENTORY <-> INVENTORY
             * =========================================================
             *
             * Both DB IDs remain in PlayerInventory.
             * This is strictly presentation.
             */

            if (!thisEquipped && !otherEquipped) {

                if (!EquipmentDatabase.TryFind(Equipment, out EquipmentBase mine)) {

                    return false;
                }

                if (!EquipmentDatabase.TryFind(otherSlot.Equipment, out EquipmentBase theirs)) {

                    return false;
                }

                BindDatabaseReference(theirs, otherSlot);

                otherSlot.BindDatabaseReference(mine, this);

                return true;
            }


            /*
             * =========================================================
             * EQUIPPED <-> EQUIPPED
             * =========================================================
             *
             * Both live objects remain equipped.
             * Game.PlayerEquipment doesn't change.
             */

            if (thisEquipped && otherEquipped) {

                if (Owner == null || Owner != otherSlot.Owner) {

                    Debug.LogWarning("Cross-Pawn equipped-slot swapping is not supported.");

                    return false;
                }

                EquipmentBase mine = Equipment;

                EquipmentBase theirs = otherSlot.Equipment;

                BindLiveReference(theirs, otherSlot);

                otherSlot.BindLiveReference(mine, this);

                return true;
            }


            /*
             * =========================================================
             * INVENTORY <-> EQUIPPED
             * =========================================================
             *
             * THIS is the real gameplay swap.
             */

            UIEquipmentSlot equippedSlot = thisEquipped ? this : otherSlot;

            UIEquipmentSlot inventorySlot = thisEquipped ? otherSlot : this;

            Pawn owner = equippedSlot.Owner;

            if (owner == null)
                return false;


            EquipmentBase oldLiveEquipment = equippedSlot.Equipment;

            if (!EquipmentDatabase.TryFind(oldLiveEquipment, out EquipmentBase oldDBEquipment)) {

                return false;
            }


            if (!EquipmentDatabase.TryFind(inventorySlot.Equipment, out EquipmentBase incomingDBEquipment)) {

                return false;
            }


            /*
             * Create the incoming LIVE representation.
             */
            if (!EquipmentDatabase.TryCreateInstance(incomingDBEquipment, out EquipmentBase newLiveEquipment, equippedSlot.transform)) {

                return false;
            }


            /*
             * Pawn performs the WHOLE authoritative transaction:
             *
             * Game.PlayerInventory:
             *     remove incoming ID
             *     add old equipped ID
             *
             * Game.PlayerEquipment:
             *     replace old ID with incoming ID
             *
             * Runtime:
             *     destroy old live object
             *     install new live object
             *     replace modifiers/events
             */
            if (!owner.TryEquip(newLiveEquipment, true)) {

                Destroy(newLiveEquipment.gameObject);

                return false;
            }


            /*
             * Only now that gameplay succeeded do we commit UI.
             */
            equippedSlot.BindLiveReference(newLiveEquipment, inventorySlot);

            inventorySlot.BindDatabaseReference(oldDBEquipment, equippedSlot);

            return true;
        }


        public bool CanSwapWith(Object target) {
            return target switch {
                UIEquipmentSlot slot => slot.CheckEquipType(Equipment),

                UIItemSlot => true,

                _ => false
            };
        }
        #endregion


        public EquipmentBase GetEquipmentInfo() =>
            Equipment;


        #region Drag / Drop
        public override void OnDrop(PointerEventData eventData) {

            UIEquipmentSlot source = eventData.pointerPress != null ? eventData.pointerPress.GetComponent<UIEquipmentSlot>() : null;

            if (source == null || !source.IsAssigned() || !source.dragAndDropEnabled) {

                return;
            }

            source.dropPreformed = true;

            if (!enabled || !m_DragAndDropEnabled) {

                return;
            }

            bool assignSuccess = false;


            /*
             * EMPTY DESTINATION
             *
             * Assign(source) performs the entire move and clears the
             * source reference itself.
             */
            if (!IsAssigned()) {
                assignSuccess = Assign(source);
            }
            /*
             * OCCUPIED DESTINATION
             */
            else {
                if (!isStatic && !source.isStatic) {

                    if (CanSwapWith(source) && source.CanSwapWith(this)) {

                        assignSuccess = source.PerformSlotSwap(this);
                    }
                }
                else if (!isStatic && source.isStatic) {

                    /*
                     * Static source gets copied rather than moved.
                     */
                    assignSuccess = Assign(source);
                }
            }


            if (!assignSuccess) {
                OnAssignBySlotFailed(source);
            }
        }
        #endregion


        #region Tooltip
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
        #endregion


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