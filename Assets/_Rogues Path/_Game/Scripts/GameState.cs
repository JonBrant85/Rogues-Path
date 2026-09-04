using System.Collections.Generic;
using System.Linq;
using _Rogues_Path.Crafting;
using _Rogues_Path.Equipment.Scripts;
using _Rogues_Path.UI.ActionBar;
using _Rogues_Path.UI.MenuBar;
using _Rogues_Path.UI.RewardsScreen;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
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
        RewardsScreen,
        GameOver
    }

    public enum Trigger {
        EnterInitialLoad,
        InitialLoadComplete,
        EnterMainMenu,
        EnterCharacterSelection,
        EnterLevelSelection,
        EnterCombat,
        EnterRewardsScreen,
        EnterWorld,
        GameOver
    }

    public partial class Game {
        private StateMachine<State, Trigger> gameState = new StateMachine<State, Trigger>(State.Boot);

        public void InitGameState() {
            gameState.Configure(State.Boot).Permit(Trigger.EnterInitialLoad, State.InitialLoad);

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
                .Permit(Trigger.EnterWorld, State.WorldMap);

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
                .Permit(Trigger.EnterRewardsScreen, State.RewardsScreen)
                .Permit(Trigger.GameOver, State.GameOver);

            gameState.Configure(State.RewardsScreen)
                .OnEntry(
                    () => {
                        // Prepare Rewards
                        for (int i = 0; i < 2; i++) {
                            if (EquipmentDatabase.GetIDByName(EquipmentDatabase.Instance.Equipment.GetRandomElement().Name, out int ID)) {
                                Instance.PendingEquipmentRewards.Add(ID);
                            }
                        }

                        Orb orb = OrbDatabase.Instance.GetRandomOrb();

                        if (Instance.PendingOrbRewards.ContainsKey(orb)) {
                            PendingOrbRewards[orb]++;
                        }
                        else {
                            PendingOrbRewards.Add(orb, 1);
                        }

                        LoadingScreenManager.Instance.LoadScene(Rewards);

                        EventBus.Raise(new InventoryChanged());
                    })
                .OnExit(
                    () => {
                        UIRewardsScreen.Hide();
                    })
                .Permit(Trigger.EnterWorld, State.WorldMap);

            gameState.Configure(State.WorldMap)
                .OnEntry(
                    () => {
                        LoadingScreenManager.Instance.LoadScene(World);
                    })
                .Permit(Trigger.EnterCombat, State.Combat);

            gameState.Configure(State.GameOver)
                .OnEntry(
                    () => {
                        LoadingScreenManager.Instance.LoadScene(GameOverScene);
                    })
                .Permit(Trigger.EnterMainMenu, State.MainMenu);
        }

        public static void FireTrigger(Trigger trigger) {
            Instance.gameState.Fire(trigger);
        }
    }
}
