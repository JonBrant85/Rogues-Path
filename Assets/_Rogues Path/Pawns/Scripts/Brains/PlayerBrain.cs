using System.Collections.Generic;
using _Rogues_Path._Game;
using _Rogues_Path.Combat;
using _Rogues_Path.Commands;
using _Rogues_Path.Utilities;
using Cysharp.Threading.Tasks;
using DuloGames.UI;
using UnityEngine;

namespace _Rogues_Path.Pawns.Brains {
    public class PlayerBrain : PawnBrain {
        public List<UISpellInfo> Spells = new();

        public override async UniTask HandleTurn() {
            await Game.CommandInvoker.ExecuteCommand(
                new List<Command> {
                    Spells.GetRandomElement()
                        .SpellCommand
                },
                new CommandContext {
                    Caster = Owner,
                    Targets = new List<Pawn> {
                        CombatManager.Instance.Enemy
                    }
                });
        }
    }
}