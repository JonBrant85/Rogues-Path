using System.Collections.Generic;
using _Rogues_Path._Game;
using DuloGames.UI;
using UnityEngine;

namespace _Rogues_Path.Pawns {
    [CreateAssetMenu(fileName = "New " + nameof(PawnData), menuName = Game.Name + "/Data/" +nameof(PawnData))]
    public class PawnData: ScriptableObject {
        public string Name;
        public string ClassName;
        public Pawn TwoDPawn;
        public FourDPawn FourDPawn;

        public List<UISpellInfo> ClassSpells = new();
    }
}