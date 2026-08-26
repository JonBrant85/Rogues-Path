using _Rogues_Path.PawnEquipment.Scripts;
using Assets.HeroEditor4D.Common.Scripts.Enums;

namespace _Rogues_Path.Utilities {
    public static class EquipmentPartExtensions {
        public static EquipmentPartMask ToMask(this EquipmentPart part) {
            return (EquipmentPartMask)(1 << (int)part);
        }

        public static bool Accepts(this EquipmentPartMask mask, EquipmentPart part) {
            return (mask & part.ToMask()) != 0;
        }
    }
}