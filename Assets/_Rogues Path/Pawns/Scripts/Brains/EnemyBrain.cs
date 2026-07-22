using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using _Rogues_Path._Game;
using _Rogues_Path.Combat;
using _Rogues_Path.Commands;
using _Rogues_Path.Utilities;
using Cysharp.Threading.Tasks;

namespace _Rogues_Path.Pawns.Brains {
    public class EnemyBrain : PawnBrain {
        public List<Command> PotentialCommands = new();

        public override async UniTask HandleTurn() {
            // If Enemy is dead, do nothing
            if (Owner.IsDead) return;

            // If all players are dead, do nothing
            if (CombatManager.Instance.Player.IsDead) return;

            await Game.CommandInvoker.ExecuteCommand(
                new List<Command> {
                    PotentialCommands.GetRandomElement()
                },
                new CommandContext {
                    Caster = Owner,
                    Targets = new List<Pawn> {
                        CombatManager.Instance.Player
                    }
                });
        }
    }
}