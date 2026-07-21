using System.Collections;
using System.Collections.Generic;
using _Rogues_Path._Game;
using _Rogues_Path.Commands;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CommandInvoker {
        public int QueueCount { get { return commandQueue.Count; } }
        private Queue<(Command, CommandContext)> commandQueue = new();
        
        public async UniTask ExecuteCommand(List<Command> commands, CommandContext context) {
            foreach (var command in commands) {
                commandQueue.Enqueue((command, context));
            }

            while (commandQueue.Count > 0) {
                var queueElement = commandQueue.Dequeue();
                var queueCommand = queueElement.Item1;
                var queueContext = queueElement.Item2;
                await queueCommand.Execute(queueContext.Caster, queueContext.Targets);
            } 

            /*
            bool allPlayersDead = Game.PlayerPawns.TrueForAll(player => player.IsDead);
            bool allEnemiesDead = Game.EnemyPawns.TrueForAll(enemy => enemy.IsDead);

            if (allEnemiesDead) {
                Game.FireTrigger(GameStateTriggers.EnterRewards);
            }

            if (allPlayersDead) {
                Game.FireTrigger(GameStateTriggers.GameOver);
            }
            */
        }
    }