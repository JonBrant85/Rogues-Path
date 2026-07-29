using System.Collections.Generic;
using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.LevelSelection;
using _Rogues_Path.Pawns;
using _Rogues_Path.Utilities;
using DuloGames.UI;
using HeroEditor.Common.Enums;
using Sirenix.OdinInspector;

namespace _Rogues_Path._Game {
    public partial class Game {
        [FoldoutGroup("Data")] public Pawn CurrentCharacter;
        [FoldoutGroup("Data")] public LevelData CurrentLevel;
        [FoldoutGroup("Data")] public EquipmentPartIntDictionary PlayerEquipment = new();
        [FoldoutGroup("Data")] public List<int> PlayerInventory = new();
        [FoldoutGroup("Data")] public List<int>  PendingRewards;
    }
}