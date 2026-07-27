using System;
using _Rogues_Path._Game;
using UnityEngine;

namespace _Rogues_Path.UI.CharacterScreen {
    [CreateAssetMenu(fileName = nameof(CharacterStatID), menuName = Game.Name + "/Data/" + nameof(CharacterStatID)), Serializable]
    public class CharacterStatID : ScriptableObject {}
}