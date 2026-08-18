using System.Collections.Generic;
using _Rogues_Path._Game;
using _Rogues_Path.Equipment.Scripts;
using DuloGames.UI;
using UnityEngine;

namespace _Rogues_Path.Pawns.Scripts {
    [CreateAssetMenu(fileName = "New " + nameof(PawnData), menuName = Game.Name + "/Data/" +nameof(PawnData))]
    public class PawnData: ScriptableObject {
        public string Name;
        public string ClassName;
        public Pawn Pawn;

        public List<UISpellInfo> ClassSpells = new();
        public List<EquipmentBase> StartingEquipment = new();
    }
}