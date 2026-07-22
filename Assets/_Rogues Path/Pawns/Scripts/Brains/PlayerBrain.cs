using System.Collections.Generic;
using _Rogues_Path._Game;
using _Rogues_Path.Combat;
using _Rogues_Path.Commands;
using _Rogues_Path.UI.ActionBar;
using _Rogues_Path.Utilities;
using Cysharp.Threading.Tasks;
using DuloGames.UI;
using UnityEngine;

namespace _Rogues_Path.Pawns.Brains {
    public class PlayerBrain : PawnBrain {
        public override async UniTask HandleTurn() {
            // If player is dead, do nothing
            if (Owner.IsDead) return;

            // If all enemies are dead, do nothing
            if (CombatManager.Instance.Enemy.IsDead) return;

            // Ready the first available spell, return if none found
            var spellToCast = UIActionBar.Instance.GetFirstSpellOffCooldown();
            if (spellToCast == null) return;

            await Game.CommandInvoker.ExecuteCommand(
                new List<Command> {

                    spellToCast.SpellCommand
                },
                new CommandContext {
                    Caster = Owner,
                    Targets = new List<Pawn> {
                        CombatManager.Instance.Enemy
                    }
                });

            UIActionBar.Instance.TriggerSpellCooldown(spellToCast);
        }
    }
}