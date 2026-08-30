using _Rogues_Path.Pawns.Scripts;
using Assets.HeroEditor4D.Common.Scripts.Enums;

namespace _Rogues_Path.Utilities.Events {
    public struct EquipmentUnequippedEvent : IEvent {
        public Pawn Owner;
        public EquipmentPart EquipType;
    }
}