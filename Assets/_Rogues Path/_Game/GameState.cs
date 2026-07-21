using System.Linq;
using Michsky.LSS;
using Stateless;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace _Rogues_Path._Game {
    public enum State {
        Boot,
        InitialLoad,
        MainMenu,
        CharacterSelection,
        LevelSelection,
        WorldMap,
        Combat
    }

    public enum Trigger {
        EnterInitialLoad,
        InitialLoadComplete,
        EnterMainMenu,
        EnterCharacterSelection,
        EnterLevelSelection,
        EnterWorldMap,
        EnterCombat
    }

    public partial class Game {
        private StateMachine<State, Trigger> gameState = new StateMachine<State, Trigger>(State.Boot);

        public void InitGameState() {
            gameState.Configure(State.Boot)
                .Permit(Trigger.EnterInitialLoad, State.InitialLoad);

            gameState.Configure(State.InitialLoad)
                .OnEntry(
                    () => {
                        Debug.Log($"Loading Main Menu");
                        FireTrigger(Trigger.EnterMainMenu);
                    })
                .Permit(Trigger.EnterMainMenu, State.MainMenu);

            gameState.Configure(State.MainMenu)
                .OnEntry(
                    () => {
                        LoadingScreenManager.Instance.LoadScene(MainMenu);
                    })
                .Permit(Trigger.EnterCharacterSelection, State.CharacterSelection);

            gameState.Configure(State.CharacterSelection)
                .OnEntry(
                    () => {
                        Debug.Log($"Entering Character Selection");
                        LoadingScreenManager.Instance.LoadScene(CharacterSelection);
                    })
                .Permit(Trigger.EnterMainMenu, State.MainMenu)
                .Permit(Trigger.EnterLevelSelection, State.LevelSelection);

            gameState.Configure(State.LevelSelection)
                .OnEntry(
                    () => {
                        Debug.Log($"Entering Level Selection");
                        LoadingScreenManager.Instance.LoadScene(LevelSelection);
                    })
                .Permit(Trigger.EnterWorldMap, State.WorldMap)
                .Permit(Trigger.EnterCharacterSelection, State.CharacterSelection);
        }

        public void FireTrigger(Trigger trigger) {
            Debug.Assert(gameState.PermittedTriggers.Contains(trigger));
            gameState.Fire(trigger);
        }
    }
}