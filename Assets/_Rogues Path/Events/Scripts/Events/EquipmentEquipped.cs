using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.Pawns;

namespace _Rogues_Path.Utilities.Events {
    public struct EquipmentEquippedEvent: IEvent {
        public EquipmentBase Equipment;
        public Pawn Owner;
    }
}