using System.Collections.Generic;
using System.Linq;
using _Rogues_Path._Game;
using UnityEngine;

namespace _Rogues_Path.Buffs.Scripts {
    [CreateAssetMenu(menuName = Game.Name + "/" + nameof(BuffsDatabase), fileName = nameof(BuffsDatabase))]
    public class BuffsDatabase : ScriptableObject{
        #region singleton
        private static BuffsDatabase m_Instance;
        public static BuffsDatabase Instance {
            get {
                if (m_Instance == null)
                    m_Instance = Resources.Load("Databases/BuffsDatabase") as BuffsDatabase;

                return m_Instance;
            }
        }
        #endregion

        [SerializeField] private List<PawnBuff> buffs = new();

        public bool TryGetBuffByName(string query, out PawnBuff buff) {
            var matchingBuff = buffs.FirstOrDefault(pawnBuff =>pawnBuff.Name == query);
            buff = matchingBuff;
            return matchingBuff != null;
        }
    }
}