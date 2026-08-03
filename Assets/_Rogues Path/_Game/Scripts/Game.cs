using System;
using _Rogues_Path.LevelSelection;
using _Rogues_Path.Pawns;
using _Rogues_Path.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Rogues_Path._Game {
    public partial class Game : Singleton<Game> {
        public const string Name = "Rogue's Path";
        [ReadOnly] public string CurrentState;
        public static CommandInvoker CommandInvoker { get { return Instance.commandInvoker ?? (Instance.commandInvoker = new CommandInvoker()); } }
        private CommandInvoker commandInvoker;

        [SerializeField, FoldoutGroup("Scenes")] private SceneField MainMenu;
        [SerializeField, FoldoutGroup("Scenes")] private SceneField CharacterSelection;
        [SerializeField, FoldoutGroup("Scenes")] private SceneField LevelSelection;
        [SerializeField, FoldoutGroup("Scenes")] private SceneField Combat;
        [SerializeField, FoldoutGroup("Scenes")] private SceneField Rewards;
        [SerializeField, FoldoutGroup("Scenes")] private SceneField World;
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