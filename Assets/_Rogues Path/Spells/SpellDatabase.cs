using System;
using System.Collections.Generic;
using _Rogues_Path._Game;
using DuloGames.UI;
using UnityEngine;

namespace _Rogues_Path.Spells {
    [Serializable]
    public class SpellDatabaseEntry {
        [HideInInspector] public string Guid;
        public int ID = -1;
        public UISpellInfo Spell;
    }

    [CreateAssetMenu(menuName = Game.Name + "/" + nameof(SpellDatabase), fileName = nameof(SpellDatabase))]
    public class SpellDatabase : ScriptableObject {
        private static SpellDatabase m_Instance;

        public static SpellDatabase Instance {
            get {
                if (m_Instance == null) {
                    m_Instance = Resources.Load<SpellDatabase>("SpellDatabase");
                }

                return m_Instance;
            }
        }

        [SerializeField] private List<SpellDatabaseEntry> spells = new();

        public static bool TryGetByID(int id, out UISpellInfo spell) {
            spell = null;

            if (Instance == null)
                return false;

            SpellDatabaseEntry entry = Instance.spells.Find(x => x.ID == id);

            if (entry?.Spell == null)
                return false;

            spell = entry.Spell;
            return true;
        }

        public bool TryGetID(UISpellInfo spell, out int id) {
            id = -1;

            if (spell == null)
                return false;

            SpellDatabaseEntry entry = spells.Find(x => x.Spell == spell);

            if (entry == null)
                return false;

            id = entry.ID;
            return true;
        }

    #if UNITY_EDITOR
        private void OnValidate() {
            HashSet<int> usedIDs = new();

            int nextID = 0;

            foreach (SpellDatabaseEntry entry in spells) {
                if (string.IsNullOrEmpty(entry.Guid)) {
                    entry.Guid = Guid.NewGuid().ToString();
                }

                if (entry.ID >= 0 && usedIDs.Add(entry.ID)) {
                    nextID = Mathf.Max(nextID, entry.ID + 1);
                    continue;
                }

                while (usedIDs.Contains(nextID)) {
                    nextID++;
                }

                entry.ID = nextID;
                usedIDs.Add(nextID);

                nextID++;
            }
        }
    #endif
    }
}