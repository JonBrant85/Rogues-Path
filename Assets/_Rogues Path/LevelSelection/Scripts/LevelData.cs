using System.Collections.Generic;
using _Rogues_Path._Game;
using _Rogues_Path.Pawns;
using _Rogues_Path.Pawns.Scripts;
using UnityEngine;

namespace _Rogues_Path.LevelSelection {
    [CreateAssetMenu(fileName = "New Level Data", menuName = "Rogue's Path/Data/LevelData")]
    public class LevelData : ScriptableObject {
        public string LevelName;
        public string Description;
        public GameObject BackgroundPrefab;
        public List<PawnData> Enemies = new();
    }
}