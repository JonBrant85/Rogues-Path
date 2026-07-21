using System;
using System.Collections.Generic;
using System.Reflection;

namespace _Rogues_Path.ServiceProviders {
    public static class Services {
        private static readonly Dictionary<Type, object> _services = new(capacity: 16);

        public static void Init() {
            _services.Clear();
        }

        public static void Add<T>(T service) {
            _services.Add(service.GetType(), service);
        }

        public static void Add(Type type, object service) {
            _services.Add(type, service);
        }

        public static bool Remove<T>() {
            return _services.Remove(typeof(T));
        }

        public static bool Remove(Type type) {
            return _services.Remove(type);
        }

        public static T Get<T>() {
            if (!_services.TryGetValue(typeof(T), out var service)) {
                throw new Exception($"Service of type {typeof(T)} not found");
            }

            if (service is not T typedService) {
                throw new Exception($"Service of type {service.GetType()} is not of type {typeof(T)}");
            }

            return typedService;
        }

        public static object Get(Type type) {
            if (!_services.TryGetValue(type, out var service)) {
                throw new Exception($"Service of type {type} not found");
            }

        #if UNITY_EDITOR
            if (service.GetType() != type) {
                throw new Exception($"Service of type {service.GetType()} is not of type {type}");
            }
        #endif

            return service;
        }

        public static bool Has<T>() {
            return _services.ContainsKey(typeof(T));
        }

        public static bool Has(Type type) {
            return _services.ContainsKey(type);
        }

        public static void Inject(object obj) {
            var fields = obj.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (var field in fields) {
                if (field.GetCustomAttribute<InjectAttribute>() == null) continue;

                var service = Get(field.FieldType);
                field.SetValue(obj, service);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class InjectAttribute : Attribute {}
}