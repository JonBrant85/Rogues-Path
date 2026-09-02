using System.Collections.Generic;
using _Rogues_Path.Combat;
using _Rogues_Path.Crafting;
using _Rogues_Path.LevelSelection;
using _Rogues_Path.Pawns.Scripts;
using _Rogues_Path.Utilities;
using DuloGames.UI;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Rogues_Path._Game {
    public partial class Game {
        [FoldoutGroup("Data")] public PawnData PlayerData;
        [FoldoutGroup("Data")] public List<UISpellInfo> PlayerPreparedSpells = new();
        [FoldoutGroup("Data")] public List<int> ActionBarSpellOrder = new();
        [FoldoutGroup("Data")] public LevelData LevelData;
        [FoldoutGroup("Data")] public float PlayerCurrentHealth = -1f;
        [FoldoutGroup("Data")] public EquipmentPartInstanceDictionary PlayerEquipment = new();
        [FoldoutGroup("Data")] public List<EquipmentInstanceData> PlayerInventory = new();
        [FoldoutGroup("Data")] public OrbCountDictionary PlayerOrbs = new();
        [FoldoutGroup("Data")] public List<int> PendingEquipmentRewards;
        public Dictionary<Orb, int> PendingOrbRewards = new();

        [Button("Give 50 Health")]
        private void GiveDebugHealth() {
            Pawn player = CombatManager.Instance.Player;

            if (player == null) {
                Debug.LogWarning("Give 50 Health requires an active combat player.");
                return;
            }

            float previousHealth = player.CurrentHealth;
            player.TakeDamage(-50, player);
            PlayerHealthState.Save(player);

            Debug.Log($"Gave player 50 health. Health={previousHealth:0.#}->{player.CurrentHealth:0.#}");
        }

        public int GetOrbCount(int orbID) {
            return PlayerOrbs.GetValueOrDefault(orbID);
        }

        public int GetOrbCount(Orb orb) {
            if (orb == null)
                return 0;

            if (!OrbDatabase.Instance.TryGetID(orb, out int orbID)) {

                return 0;
            }

            return PlayerOrbs.TryGetValue(orbID, out int count) ? count : 0;
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

        public bool AddOrb(Orb orb, int amount = 1) {

            if (orb == null || amount <= 0)
                return false;

            if (!OrbDatabase.Instance.TryGetID(orb, out int orbID)) {

                return false;
            }

            if (PlayerOrbs.ContainsKey(orbID))
                PlayerOrbs[orbID] += amount;
            else
                PlayerOrbs.Add(orbID, amount);

            return true;
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

        public bool TryConsumeOrb(Orb orb, int amount = 1) {

            if (orb == null || amount <= 0)
                return false;

            if (!OrbDatabase.Instance.TryGetID(orb, out int orbID)) {

                return false;
            }

            if (!PlayerOrbs.TryGetValue(orbID, out int count) || count < amount) {

                return false;
            }

            count -= amount;

            if (count <= 0)
                PlayerOrbs.Remove(orbID);
            else
                PlayerOrbs[orbID] = count;

            return true;
        }
    }
}
