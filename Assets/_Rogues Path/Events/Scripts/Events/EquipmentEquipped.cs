using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.Pawns;
using _Rogues_Path.Pawns.Scripts;

namespace _Rogues_Path.Utilities.Events {
    public struct EquipmentEquippedEvent: IEvent {
        public EquipmentBase Equipment;
        public Pawn Owner;
    }
}