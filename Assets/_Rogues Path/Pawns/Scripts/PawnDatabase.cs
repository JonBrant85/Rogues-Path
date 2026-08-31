using System.Collections.Generic;
using _Rogues_Path._Game;
using _Rogues_Path.Pawns.Scripts;
using UnityEngine;

namespace _Rogues_Path.Pawns {
    [CreateAssetMenu(menuName = Game.Name + "/Databases/Pawn Database", fileName = "PawnDatabase")]
    public class PawnDatabase : ScriptableObject {
        private static PawnDatabase instance;

        public static PawnDatabase Instance {
            get {
                if (instance == null) {
                    instance = Resources.Load<PawnDatabase>("PawnDatabase");
                }

                return instance;
            }
        }

        public List<PawnData> Pawns = new();

        public bool TryGetByID(int id, out PawnData pawn) {
            pawn = null;

            if (id < 0 || id >= Pawns.Count)
                return false;

            pawn = Pawns[id];
            return pawn != null;
        }

        public bool TryGetID(PawnData pawn, out int id) {
            id = Pawns.IndexOf(pawn);
            return id >= 0;
        }
    }
}