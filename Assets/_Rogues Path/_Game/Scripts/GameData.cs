using System.Collections.Generic;
using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.LevelSelection;
using _Rogues_Path.Pawns;
using _Rogues_Path.Pawns.Scripts;
using _Rogues_Path.Utilities;
using DuloGames.UI;
using HeroEditor.Common.Enums;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

namespace _Rogues_Path._Game {
    public partial class Game {
        [FoldoutGroup("Data")] public PawnData PlayerData;
        [FoldoutGroup("Data")] public LevelData LevelData;
        [FoldoutGroup("Data")] public EquipmentPartIntDictionary PlayerEquipment = new();
        [FoldoutGroup("Data")] public List<int> PlayerInventory = new();
        [FoldoutGroup("Data")] public List<int>  PendingRewards;
    }
}