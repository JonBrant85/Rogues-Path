using System;
using Assets.HeroEditor4D.Common.Scripts.Enums;

namespace _Rogues_Path.PawnEquipment.Scripts {
    [Flags]
    public enum EquipmentPartMask {
        None = 0,

        Armor = 1 << 0,
        Helmet = 1 << 1,
        Vest = 1 << 2,
        Bracers = 1 << 3,
        Leggings = 1 << 4,
        MeleeWeapon1H = 1 << 5,
        MeleeWeapon2H = 1 << 6,
        Bow = 1 << 7,
        Crossbow = 1 << 8,
        SecondaryMelee1H = 1 << 9,
        SecondaryFirearm1H = 1 << 10,
        Shield = 1 << 11,
        Earrings = 1 << 12,
        Cape = 1 << 13,
        Quiver = 1 << 14,
        Back = 1 << 15,
        Mask = 1 << 16,
        Firearm1H = 1 << 17,
        Firearm2H = 1 << 18,
        Wings = 1 << 19,

        All = (1 << 20) - 1
    }
}