using System.Collections.Generic;
using _Rogues_Path.Pawns;
using UnityEngine;

namespace _Rogues_Path.Utilities.Events {
    public class CombatEncounterStarted : IEvent {
        public GameObject BackgroundPrefab;
        public List<PawnData> Enemies;
    }
}