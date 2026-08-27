using System.Collections.Generic;
using _Rogues_Path.Crafting;
using _Rogues_Path.LevelSelection;
using _Rogues_Path.Pawns.Scripts;
using _Rogues_Path.Utilities;
using Sirenix.OdinInspector;

namespace _Rogues_Path._Game {
    public partial class Game {
        [FoldoutGroup("Data")] public PawnData PlayerData;
        [FoldoutGroup("Data")] public LevelData LevelData;
        [FoldoutGroup("Data")] public EquipmentPartInstanceDictionary PlayerEquipment = new();
        [FoldoutGroup("Data")] public List<EquipmentInstanceData> PlayerInventory = new();
        [FoldoutGroup("Data")] public Dictionary<int, int> PlayerOrbs = new();
        [FoldoutGroup("Data")] public List<int> PendingRewards;


        public int GetOrbCount(int orbID) {
            return PlayerOrbs.GetValueOrDefault(orbID);
        }

        public int GetOrbCount(Orb orb) {
            if (!OrbDatabase.TryGetID(orb, out int orbID))
                return 0;

            return GetOrbCount(orbID);
        }

        public bool AddOrb(int orbID, int count = 1) {
            if (count <= 0)
                return false;

            if (!OrbDatabase.TryGetByID(orbID, out _))
                return false;

            if (!PlayerOrbs.TryAdd(orbID, count)) {
                PlayerOrbs[orbID] += count;
            }

            return true;
        }

        public bool AddOrb(Orb orb, int count = 1) {
            if (!OrbDatabase.TryGetID(orb, out int orbID))
                return false;

            return AddOrb(orbID, count);
        }

        public bool TryConsumeOrb(int orbID, int count = 1) {
            if (count <= 0)
                return false;

            if (!PlayerOrbs.TryGetValue(orbID, out int currentCount))
                return false;

            if (currentCount < count)
                return false;

            currentCount -= count;

            if (currentCount == 0) {
                PlayerOrbs.Remove(orbID);
            }
            else {
                PlayerOrbs[orbID] = currentCount;
            }

            return true;
        }

        public bool TryConsumeOrb(Orb orb, int count = 1) {
            if (!OrbDatabase.TryGetID(orb, out int orbID))
                return false;

            return TryConsumeOrb(orbID, count);
        }
    }
}