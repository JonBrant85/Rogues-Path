using _Rogues_Path.Crafting;
using _Rogues_Path.UI.Slots;

namespace _Rogues_Path.Utilities.Events {
    public struct EquipmentSlotClicked: IEvent {
        public UIEquipmentSlot Slot;
        public EquipmentInstanceData InstanceData;

        public EquipmentSlotClicked(UIEquipmentSlot slot, EquipmentInstanceData instanceData) {
            Slot = slot;
            InstanceData = instanceData;
        }
    }
}