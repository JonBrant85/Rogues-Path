using System.Collections.Generic;
using _Rogues_Path._Game;
using _Rogues_Path.Combat;
using _Rogues_Path.Commands;
using _Rogues_Path.Utilities;
using Cysharp.Threading.Tasks;
using DuloGames.UI;
using UnityEngine;

namespace _Rogues_Path.Pawns.Scripts.Brains {
    public class EnemyBrain : PawnBrain {
        private readonly Dictionary<UISpellInfo, float> spellCooldownEndTimes = new();

        public override async UniTask HandleTurn() {
            // If Enemy is dead, do nothing
            if (Owner.IsDead) return;

            // If all players are dead, do nothing
            if (CombatManager.Instance.Player.IsDead) return;

            UISpellInfo spellToCast = GetRandomSpellOffCooldown();
            if (spellToCast == null) return;

            await Game.CommandInvoker.ExecuteCommand(
                new List<Command> {
                    spellToCast.SpellCommand
                },
                new CommandContext {
                    Caster = Owner,
                    Targets = new List<Pawn> {
                        CombatManager.Instance.Player
                    }
                });

            spellCooldownEndTimes[spellToCast] = Time.time + Mathf.Max(0, spellToCast.Cooldown);
        }

        private UISpellInfo GetRandomSpellOffCooldown() {
            List<UISpellInfo> availableSpells = new();

            foreach (UISpellInfo spell in KnownSpells) {
                if (spell == null || spell.SpellCommand == null)
                    continue;

                if (spellCooldownEndTimes.TryGetValue(spell, out float cooldownEndTime) && cooldownEndTime > Time.time)
                    continue;

                availableSpells.Add(spell);
            }

            return availableSpells.Count > 0 ? availableSpells.GetRandomElement() : null;
        }
    }
}
