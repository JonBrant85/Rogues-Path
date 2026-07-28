using System.Linq;
using _Rogues_Path.UI.ActionBar;
using _Rogues_Path.UI.MenuBar;
using _Rogues_Path.UI.RewardsScreen;
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
        Combat,
        RewardsScreen
    }

    public enum Trigger {
        EnterInitialLoad,
        InitialLoadComplete,
        EnterMainMenu,
        EnterCharacterSelection,
        EnterLevelSelection,
        EnterWorldMap,
        EnterCombat,
        EnterRewardsScreen,
        GameOver
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
                .Permit(Trigger.EnterMainMenu, State.MainMenu)
                .Permit(Trigger.EnterRewardsScreen, State.RewardsScreen);

            gameState.Configure(State.RewardsScreen)
                .OnEntry(
                    () => {
                        UIActionBar.Hide();
                        UIMenuBar.Hide();
                        UIRewardsScreen.Show();
                    })
                .PermitReentry(Trigger.EnterRewardsScreen)
                .Permit(Trigger.EnterCombat, State.Combat);
        }

        public static void FireTrigger(Trigger trigger) {
            Debug.Assert(Instance.gameState.PermittedTriggers.Contains(trigger));
            Instance.gameState.Fire(trigger);
        }
    }
}