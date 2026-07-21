using System;
using System.Collections.Generic;
using HeroEditor.Common.Enums;
using UnityEngine;

namespace _Rogues_Path.Utilities {
    [Serializable] public class BuffsDictionary : UnitySerializedDictionary<string, int> {}

    /*
    [Serializable] public class EquipmentDictionary : UnitySerializedDictionary<EquipmentPart, EquipmentBase> {
        public EquipmentDictionary(EquipmentDictionary currentEquipment = null) {
            keyValueData = new List<KeyValueData>();
            if (currentEquipment == null) return;


            string logString = $"Creating a copy of a dictionary. Source has {currentEquipment.Count} items. ";

            foreach (var kvp in currentEquipment) {
                keyValueData.Add(
                    new KeyValueData {
                        key = kvp.Key,
                        value = kvp.Value
                    });
            }

            logString += $"Copy has {keyValueData.Count} items.";
            Debug.Log(logString);
        }
    }
    */

    public abstract class UnitySerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver {
        [SerializeField] protected List<KeyValueData> keyValueData = new();

        public void OnBeforeSerialize() {
            keyValueData.Clear();

            foreach (var kvp in this) {
                keyValueData.Add(
                    new KeyValueData() {
                        key = kvp.Key,
                        value = kvp.Value
                    });
            }
        }

        public void OnAfterDeserialize() {
            Clear();

            foreach (var item in keyValueData) {
                this[item.key] = item.value;
            }
        }

        [Serializable]
        protected struct KeyValueData {
            public TKey key;
            public TValue value;
        }
    }
}