namespace _Rogues_Path.Utilities {
    using UnityEngine;
    using GenericEventBus;

// For use with https://github.com/PeturDarri/GenericEventBus
    public interface IEvent {}

    public static class EventBus {
        private static readonly GenericEventBus<IEvent, GameObject> eventBus = new GenericEventBus<IEvent, GameObject>();

        public static void SubscribeTo<T>(GenericEventBus<IEvent>.EventHandler<T> e) where T : IEvent {
            eventBus.SubscribeTo(e);
        }

        public static void SubscribeToTarget<T>(GameObject gameObject, GenericEventBus<IEvent, GameObject>.TargetedEventHandler<T> e) where T : IEvent {
            eventBus.SubscribeToTarget(gameObject, e);
        }

        public static void UnsubscribeFromTarget<T>(GameObject gameObject, GenericEventBus<IEvent, GameObject>.TargetedEventHandler<T> e) where T : IEvent {
            eventBus.UnsubscribeFromTarget(gameObject, e);
        }

        public static void UnsubscribeFrom<T>(GenericEventBus<IEvent>.EventHandler<T> e) where T : IEvent {
            eventBus.UnsubscribeFrom(e);
        }

        public static void Raise<T>(in T e) where T : IEvent {
            eventBus.Raise(in e);
        }

        public static void RaiseImmediately<T>(ref T e) where T : IEvent {
            eventBus.RaiseImmediately(ref e);
        }

        public static void RaiseImmediately<T>(ref T e, GameObject target, GameObject source) where T : IEvent {
            eventBus.RaiseImmediately(ref e, target, source);
        }

        public static void ConsumeCurrentEvent() {
            eventBus.ConsumeCurrentEvent();
        }
    }
}