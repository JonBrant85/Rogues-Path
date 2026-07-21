using _Rogues_Path.LevelSelection;
using _Rogues_Path.Pawns;
using Sirenix.OdinInspector;

namespace _Rogues_Path._Game {
    public partial class Game {
        [FoldoutGroup("Data")] public Pawn CurrentCharacter;
        [FoldoutGroup("Data")] public LevelData CurrentLevel;
    }
}