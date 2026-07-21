using System;
using _Rogues_Path.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Rogues_Path._Game {
    public partial class Game : Singleton<Game> {
        [ReadOnly] public string CurrentState;
        [SerializeField, FoldoutGroup("Scenes")] private SceneField MainMenu;
        [SerializeField, FoldoutGroup("Scenes")] private SceneField CharacterSelection;
        [SerializeField, FoldoutGroup("Scenes")] private SceneField LevelSelection;
        [SerializeField, FoldoutGroup("Scenes")] private SceneField WorldMap;
        [SerializeField, FoldoutGroup("Scenes")] private SceneField Combat;

        private void Awake() {
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
            InitGameState();
            DontDestroyOnLoad(gameObject);
        }

        private void Update() {
            CurrentState = gameState.State.ToString();
        }
    }
}