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
                        LoadingScreenManager.Instance.LoadScene(CharacterSelection);
                    })
                .Permit(Trigger.EnterMainMenu, State.MainMenu)
                .Permit(Trigger.EnterLevelSelection, State.LevelSelection);

            gameState.Configure(State.LevelSelection)
                .OnEntry(
                    () => {
                        LoadingScreenManager.Instance.LoadScene(LevelSelection);
                    })
                .Permit(Trigger.EnterCombat, State.Combat)
                .Permit(Trigger.EnterCharacterSelection, State.CharacterSelection);

            gameState.Configure(State.Combat)
                .OnEntry(
                    () => {
                        LoadingScreenManager.Instance.LoadScene(Combat);
                    })
                .Permit(Trigger.EnterMainMenu, State.MainMenu);
        }

        public void FireTrigger(Trigger trigger) {
            Debug.Assert(gameState.PermittedTriggers.Contains(trigger));
            gameState.Fire(trigger);
        }
    }
}