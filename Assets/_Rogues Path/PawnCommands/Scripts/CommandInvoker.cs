using System.Collections;
using System.Collections.Generic;
using _Rogues_Path._Game;
using _Rogues_Path.Combat;
using _Rogues_Path.Commands;
using _Rogues_Path.Pawns.Scripts;
using _Rogues_Path.Utilities;
using _Rogues_Path.Utilities.Events;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CommandInvoker {
    public int QueueCount { get { return commandQueue.Count; } }
    public bool IsBusy => isExecuting || commandQueue.Count > 0;

    private bool isExecuting;
    private Queue<(Command, CommandContext)> commandQueue = new();

    public async UniTask ExecuteCommand(List<Command> commands, CommandContext context) {
        foreach (var command in commands) {
            commandQueue.Enqueue((command, context));
        }

        while (commandQueue.Count > 0) {
            var queueElement = commandQueue.Dequeue();
            var queueCommand = queueElement.Item1;
            var queueContext = queueElement.Item2;
            isExecuting = true;

            try {
                await queueCommand.Execute(queueContext.Caster, queueContext.Targets);
            }
            finally {
                isExecuting = false;
            }
        }

        bool allPlayersDead = CombatManager.Instance.Player.IsDead;
        bool allEnemiesDead = CombatManager.Instance.Enemy.IsDead;

        if (allEnemiesDead) {
            PlayerHealthState.Save(CombatManager.Instance.Player);
            await UniTask.Delay(1500);
            Game.FireTrigger(Trigger.EnterRewardsScreen);
            EventBus.Raise(new CombatEncounterEnded());
        }

        if (allPlayersDead) {
            Game.FireTrigger(Trigger.GameOver);
            EventBus.Raise(new CombatEncounterEnded());
        }
    }
}
