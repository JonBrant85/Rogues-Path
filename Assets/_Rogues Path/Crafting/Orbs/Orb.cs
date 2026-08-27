using _Rogues_Path._Game;

namespace _Rogues_Path.Crafting.Orbs {
    using UnityEngine;

    namespace _Rogues_Path.Crafting {
        [CreateAssetMenu(menuName = Game.Name + "/Crafting/Orb", fileName = "New Orb")]
        public class Orb : ScriptableObject {
            public string Name;
            public string Description;
            public Sprite Icon;
        }
    }
}