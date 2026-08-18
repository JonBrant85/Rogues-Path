using System;
using System.Collections.Generic;
using _Rogues_Path.Pawns;
using _Rogues_Path.Pawns.Scripts;
using Cysharp.Threading.Tasks;

namespace _Rogues_Path.Commands {
    [Serializable]
    public abstract class Command: ICommand {
        protected readonly Pawn hero;

        protected Command(Pawn hero) {
            this.hero = hero;
        }

        public abstract UniTask Execute(Pawn instigator, List<Pawn> victims);

        public static T Create<T>(Pawn hero) where T : Command {
            return (T)System.Activator.CreateInstance(typeof(T), hero);
        }
    }
}