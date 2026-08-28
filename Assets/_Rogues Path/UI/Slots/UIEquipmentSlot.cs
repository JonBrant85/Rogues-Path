using System;
using System.Linq;
using _Rogues_Path._Game;
using _Rogues_Path.Crafting;
using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.PawnEquipment.Scripts;
using _Rogues_Path.Pawns;
using _Rogues_Path.Pawns.Scripts;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using DuloGames.UI;
using HeroEditor.Common.Enums;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
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
        [FormerlySerializedAs("AccepterEquipTypes")]
        [FormerlySerializedAs("EquipType")]
        public EquipmentPartMask AcceptedEquipTypes;
        public Pawn Owner;
        public EquipmentBase Equipment;
        [NonSerialized]
        public EquipmentInstanceData InstanceData;
        private EquipmentModifierDatabase modifierDatabase;

        public void BindInstanceData(EquipmentInstanceData instanceData, EquipmentModifierDatabase _modifierDatabase) {

            InstanceData = instanceData;
            this.modifierDatabase = _modifierDatabase;
        }

        public static void PrepareTooltip(EquipmentBase equipment, EquipmentInstanceData instanceData = null, EquipmentModifierDatabase modifierDatabase = null) {
            if (equipment == null)
                return;

            // Set the tooltip width
            if (UITooltipManager.Instance != null) {
                UITooltip.SetWidth(UITooltipManager.Instance.itemTooltipWidth);
            }

            // Set the title and description
            UITooltip.AddTitle("<color=#" + UIItemQualityColor.GetHexColor(equipment.Quality) + ">" + equipment.Name + "</color>");

            // Spacer
            UITooltip.AddSpacer();

            // Item types
            UITooltip.AddLineColumn(EquipTypeToString(equipment.EquipType), "ItemAttribute");

            UITooltip.AddLineColumn(equipment.Quality.ToString());

            if (equipment.Modifiers != null) {
                foreach (StatAndModifierPair pair in equipment.Modifiers) {
                    UITooltip.AddLineColumn(pair.StatID.name, "ItemAttribute");

                    UITooltip.AddLineColumn(pair.Modifier.Value.ToString("N1"), "ItemAttribute");
                }
            }

            if (instanceData != null && modifierDatabase != null && instanceData.CraftedModifiers != null && instanceData.CraftedModifiers.Count > 0) {

                UITooltip.AddSpacer();
                UITooltip.AddLine("CRAFTED", "ItemAttribute");

                foreach (RolledEquipmentModifier rolledModifier in instanceData.CraftedModifiers) {

                    if (!modifierDatabase.TryGetByID(rolledModifier.ModifierID, out EquipmentModifierDefinition definition)) {

                        continue;
                    }

                    string modifierName = string.IsNullOrEmpty(definition.Name) ? definition.StatID.name : definition.Name;
                    UITooltip.AddLine($"{definition.StatID.name}: +{rolledModifier.Value:N1}", "ItemAttribute");
                }
            }


            // Description
            if (!string.IsNullOrEmpty(equipment.Description)) {
                UITooltip.AddSpacer();

                UITooltip.AddLine(equipment.Description, "ItemAttribute");
            }

            // Flavor text
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

        /// <summary>
        /// Keeps equipped UI slots synchronized with the Pawn's
        /// already-existing live equipment.
        ///
        /// This is important when:
        /// - A Pawn was restored without equipment UI present.
        /// - Equipment UI is created after the Pawn.
        /// - A scene restores Game.PlayerEquipment before UI initializes.
        ///
        /// This does NOT equip anything and does NOT modify game state.
        /// </summary>
        private void LateUpdate() {
            SyncWithOwnerIfNeeded();
        }


        private void SyncWithOwnerIfNeeded() {
            if (!EquipToOwnerOnAssign)
                return;

            if (Owner == null)
                return;

            foreach (var pair in Owner.CurrentEquipment) {
                EquipmentPart equipType = pair.Key;
                EquipmentBase liveEquipment = pair.Value;

                if ((AcceptedEquipTypes & equipType.ToMask()) == 0)
                    continue;

                if (Equipment == liveEquipment)
                    return;

                BindLiveReference(liveEquipment, null, invokeEvents: false);

                return;
            }

            if (!ReferenceEquals(Equipment, null)) {
                ClearReferenceOnly(invokeEvents: false);
            }
        }

        public EquipmentBase GetEquipmentInfo() {
            SyncWithOwnerIfNeeded();

            return Equipment;
        }

        public bool Assign(EquipmentBase equipment) {
            return AssignDirect(equipment, null);
        }


        public bool Assign(EquipmentBase equipment, UnityEngine.Object source) {

            if (source is UIEquipmentSlot sourceSlot) {
                return AssignFromSlot(sourceSlot);
            }

            return AssignDirect(equipment, source);
        }


        public override bool Assign(UnityEngine.Object source) {
            Debug.Log($"ASSIGN OVERRIDE | " + $"Target={name} | " + $"Source={source?.name ?? "NULL"} | " + $"SourceType={source?.GetType().Name ?? "NULL"}");

            if (source is not UIEquipmentSlot sourceSlot)
                return false;

            if (sourceSlot.Equipment == null)
                return false;

            return AssignFromSlot(sourceSlot);
        }


        private bool AssignDirect(EquipmentBase equipment, Object source) {

            if (equipment == null) {
                Debug.LogError($"Attempting to assign null equipment to {AcceptedEquipTypes}.");

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
                 * -----------------------------------------------------
                 * DATABASE TEMPLATE
                 * -----------------------------------------------------
                 *
                 * EquipmentDatabase creates the LIVE instance.
                 */
                if (EquipmentDatabase.IsDatabaseEntry(equipment)) {
                    if (!EquipmentDatabase.TryCreateInstance(equipment, out EquipmentBase liveEquipment, Owner.transform)) {

                        return false;
                    }


                    /*
                     * Pawn performs the authoritative transaction.
                     *
                     * It updates:
                     * Game.PlayerEquipment
                     * Game.PlayerInventory
                     * CurrentEquipment
                     * Character
                     * modifiers
                     * event subscriptions
                     */
                    if (!Owner.TryEquip(liveEquipment, true)) {

                        Destroy(liveEquipment.gameObject);

                        return false;
                    }


                    BindLiveReference(liveEquipment, source);

                    return true;
                }


                /*
                 * -----------------------------------------------------
                 * EXISTING LIVE INSTANCE
                 * -----------------------------------------------------
                 *
                 * This is particularly important when the Pawn existed
                 * BEFORE the UI.
                 */
                if (Owner.CurrentEquipment.TryGetValue(equipment.EquipType, out EquipmentBase current) && current == equipment) {

                    BindLiveReference(equipment, source);

                    return true;
                }


                /*
                 * It's live, but not currently equipped.
                 *
                 * Let Pawn perform the normal gameplay transaction.
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
             * Inventory UI always displays DATABASE REFERENCES.
             */

            if (!EquipmentDatabase.TryFind(equipment, out EquipmentBase dbEquipment)) {

                Debug.LogError($"Could not find {equipment.Name} in EquipmentDatabase.");

                return false;
            }


            BindDatabaseReference(dbEquipment, source);

            return true;
        }


        private bool AssignFromSlot(UIEquipmentSlot sourceSlot) {
            if (sourceSlot == null || sourceSlot.Equipment == null)
                return false;

            if (sourceSlot == this)
                return true;

            bool sourceEquipped = sourceSlot.EquipToOwnerOnAssign;
            bool targetEquipped = EquipToOwnerOnAssign;

            if (targetEquipped && !CheckEquipType(sourceSlot.Equipment))
                return false;

            if (sourceSlot.isStatic)
                return AssignDirect(sourceSlot.Equipment, sourceSlot);

            return (sourceEquipped, targetEquipped) switch {
                (false, false) => MoveInventoryToInventory(),
                (false, true) => MoveInventoryToEquipped(),
                (true, false) => MoveEquippedToInventory(),
                (true, true) => MoveEquippedToEquipped()
            };

            bool MoveInventoryToInventory() {
                int sourceIndex = sourceSlot.transform.GetSiblingIndex();
                int targetIndex = transform.GetSiblingIndex();

                if (sourceIndex < 0 || sourceIndex >= Game.Instance.PlayerInventory.Count)
                    return false;

                if (targetIndex < 0 || targetIndex >= Game.Instance.PlayerInventory.Count)
                    return false;

                (Game.Instance.PlayerInventory[sourceIndex], Game.Instance.PlayerInventory[targetIndex]) =
                    (Game.Instance.PlayerInventory[targetIndex], Game.Instance.PlayerInventory[sourceIndex]);

                EventBus.Raise(new InventoryChanged());
                return true;
            }

            bool MoveInventoryToEquipped() {
                if (Owner == null)
                    return false;

                EquipmentInstanceData instanceData = sourceSlot.InstanceData;

                if (instanceData == null) {
                    Debug.LogError($"Cannot equip {sourceSlot.Equipment.Name}: " + $"source slot has no EquipmentInstanceData.");

                    return false;
                }

                EquipmentModifierDatabase sourceModifierDatabase = sourceSlot.modifierDatabase;

                if (sourceModifierDatabase == null) {
                    Debug.LogError($"Cannot equip {sourceSlot.Equipment.Name}: " + $"source slot has no EquipmentModifierDatabase.");

                    return false;
                }

                if (!EquipmentDatabase.TryCreateInstance(instanceData, sourceModifierDatabase, out EquipmentBase liveEquipment, Owner.transform)) {

                    return false;
                }

                if (!Owner.TryEquip(liveEquipment, true)) {
                    Destroy(liveEquipment.gameObject);
                    return false;
                }

                BindLiveReference(liveEquipment, sourceSlot);

                sourceSlot.ClearReferenceOnly(false);

                EventBus.Raise(new InventoryChanged());

                return true;
            }

            bool MoveEquippedToInventory() {
                if (sourceSlot.Owner == null)
                    return false;

                EquipmentBase liveEquipment = sourceSlot.Equipment;

                if (!EquipmentDatabase.TryFind(liveEquipment, out EquipmentBase dbEquipment))
                    return false;

                if (!sourceSlot.Owner.TryRemoveEquipment(liveEquipment, true))
                    return false;

                BindDatabaseReference(dbEquipment, sourceSlot);
                sourceSlot.ClearReferenceOnly(false);
                EventBus.Raise(new InventoryChanged());
                return true;
            }

            bool MoveEquippedToEquipped() {
                if (sourceSlot.Owner != Owner) {
                    Debug.LogWarning("Cross-Pawn equipped-item transfer is not supported.");
                    return false;
                }

                EquipmentBase liveEquipment = sourceSlot.Equipment;
                BindLiveReference(liveEquipment, sourceSlot);
                sourceSlot.ClearReferenceOnly(false);
                return true;
            }
        }

        public override void Unassign() {
            if (ReferenceEquals(Equipment, null))
                return;


            EquipmentBase oldEquipment = Equipment;


            /*
             * =========================================================
             * EQUIPPED SLOT
             * =========================================================
             */

            if (EquipToOwnerOnAssign && Owner != null && oldEquipment != null && !EquipmentDatabase.IsDatabaseEntry(oldEquipment)) {

                bool actuallyEquipped = Owner.CurrentEquipment.TryGetValue(oldEquipment.EquipType, out EquipmentBase current) && current == oldEquipment;


                if (actuallyEquipped) {
                    /*
                     * Gameplay transaction FIRST.
                     */
                    if (!Owner.TryRemoveEquipment(oldEquipment, true)) {

                        /*
                         * Could fail because inventory is full.
                         *
                         * Leave the UI alone if gameplay state wasn't
                         * changed.
                         */
                        return;
                    }
                }
            }


            base.Unassign();


            /*
             * Pawn may have destroyed the live Unity object already.
             * Only pass a still-valid object through the event.
             */
            if (oldEquipment != null) {
                OnUnassignEvent?.Invoke(Owner, oldEquipment);
            }


            Equipment = null;
        }

        public void ClearUIReference() {
            ClearReferenceOnly(false);

            InstanceData = null;
            modifierDatabase = null;
        }

        public override void OnTooltip(bool show) {
            UITooltip.InstantiateIfNecessary(gameObject);

            if (IsAssigned()) {
                if (show) {
                    PrepareTooltip(Equipment, InstanceData, modifierDatabase);
                    UITooltip.AnchorToRect(transform as RectTransform);
                    UITooltip.Show();
                }
                else {
                    UITooltip.Hide();
                }
            }
        }

        private void ClearReferenceOnly(bool invokeEvents = true) {

            /*
             * Use ReferenceEquals because a destroyed Unity object can
             * compare == null while the C# field still contains a
             * reference.
             */
            if (ReferenceEquals(Equipment, null))
                return;


            EquipmentBase oldEquipment = Equipment;


            base.Unassign();


            if (invokeEvents && oldEquipment != null) {

                OnUnassignEvent?.Invoke(Owner, oldEquipment);
            }


            Equipment = null;
        }

        /// <summary>
        /// Inventory slots point directly at database templates.
        /// They NEVER activate, parent or instantiate them.
        /// </summary>
        private void BindDatabaseReference(EquipmentBase dbEquipment, Object source, bool invokeEvents = true) {

            if (dbEquipment == null)
                return;


            EquipmentBase previous = Equipment;


            /*
             * Clear the previous UI state even if its Unity object was
             * destroyed elsewhere.
             */
            if (!ReferenceEquals(previous, null) && previous != dbEquipment) {

                base.Unassign();


                if (invokeEvents && previous != null) {

                    OnUnassignEvent?.Invoke(Owner, previous);
                }
            }


            Equipment = dbEquipment;


            /*
             * IMPORTANT:
             *
             * Do not:
             * - SetParent()
             * - SetActive()
             * - Instantiate()
             *
             * Database equipment stays non-live.
             */
            base.Assign(Equipment.Icon);


            if (invokeEvents) {
                OnAssignEvent?.Invoke(Owner, Equipment);


                if (source != null) {
                    OnAssignWithSourceEvent?.Invoke(this, source);
                }
            }
        }


        /// <summary>
        /// Equipped slots reference a LIVE object already owned by Pawn.
        ///
        /// The UI does NOT own the live object's transform or lifetime.
        /// </summary>
        private void BindLiveReference(EquipmentBase liveEquipment, Object source, bool invokeEvents = true) {

            if (liveEquipment == null)
                return;

            if (EquipmentDatabase.IsDatabaseEntry(liveEquipment)) {
                Debug.LogError($"Attempted to bind database template " + $"{liveEquipment.Name} as LIVE equipment in {name}.");

                return;
            }

            EquipmentBase previous = Equipment;

            if (!ReferenceEquals(previous, null) && previous != liveEquipment) {

                base.Unassign();

                if (invokeEvents && previous != null) {
                    OnUnassignEvent?.Invoke(Owner, previous);
                }
            }

            Equipment = liveEquipment;

            // THIS IS THE IMPORTANT PART
            InstanceData = liveEquipment.InstanceData;

            base.Assign(Equipment.Icon);

            if (invokeEvents) {
                OnAssignEvent?.Invoke(Owner, Equipment);

                if (source != null) {
                    OnAssignWithSourceEvent?.Invoke(this, source);
                }
            }
        }


        public override bool IsAssigned() {
            return Equipment != null;
        }

        public override void OnDrop(PointerEventData eventData) {

            UIEquipmentSlot sourceSlot = eventData.pointerPress != null ? eventData.pointerPress.GetComponent<UIEquipmentSlot>() : null;

            if (sourceSlot == null)
                return;

            if (sourceSlot == this)
                return;

            if (!sourceSlot.IsAssigned())
                return;

            if (!sourceSlot.dragAndDropEnabled || !dragAndDropEnabled)
                return;

            // Tell UISlotBase.OnEndDrag that the item was dropped
            // onto a valid slot, so it doesn't try to throw it away.
            sourceSlot.dropPreformed = true;

            bool success = AssignFromSlot(sourceSlot);


            if (!success) {
                OnAssignBySlotFailed(sourceSlot);
            }
        }

        public override void OnPointerDown(PointerEventData eventData) {

            SyncWithOwnerIfNeeded();

            base.OnPointerDown(eventData);


            OnClickEvent.Invoke(this);
        }

        public virtual bool CheckEquipType(EquipmentBase equipment) {

            if (equipment == null)
                return false;

            EquipmentPartMask equipmentMask = (EquipmentPartMask)(1 << (int)equipment.EquipType);

            return (AcceptedEquipTypes & equipmentMask) != 0;
        }
    }
}