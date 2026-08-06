using System.Threading.Tasks;
using _Rogues_Path._Game;
using _Rogues_Path.LevelSelection;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace _Rogues_Path.World.Encounters {
    public class EncounterData : ScriptableObject {
        public string EncounterTitle = "Default encounter title";

        public virtual async UniTask HandleEncounter(Transform windowContent, Transform bottomBar, Button buttonPrefab) {
            
        }
    }
}