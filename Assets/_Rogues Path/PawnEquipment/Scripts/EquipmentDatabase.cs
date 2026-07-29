using System.Collections.Generic;
using System.Linq;
using _Rogues_Path._Game;
using _Rogues_Path.Utilities;
using HeroEditor.Common.Enums;
using UnityEngine;

namespace _Rogues_Path.Equipment.Scripts {
    [CreateAssetMenu(menuName = Game.Name + "/" + nameof(EquipmentDatabase), fileName = nameof(EquipmentDatabase))]
    public class EquipmentDatabase : ScriptableObject {
        #region singleton
        private static EquipmentDatabase m_Instance;
        public static EquipmentDatabase Instance {
            get {
                if (m_Instance == null)
                    m_Instance = Resources.Load("Databases/EquipmentDatabase") as EquipmentDatabase;

                return m_Instance;
            }
        }
        #endregion

        public List<EquipmentBase> Equipment { get => new List<EquipmentBase>(equipment); }
        [SerializeField] private List<EquipmentBase> equipment = new();

        public static bool TryFind(EquipmentBase query, out EquipmentBase dbEntry) {
            dbEntry = Instance.equipment.FirstOrDefault(e => e.Name == query.Name);
            return dbEntry != null;
        }

        public static bool TryFindByName(string query, out EquipmentBase dbEntry) {
            dbEntry = Instance.equipment.FirstOrDefault(e => e.Name == query);
            return dbEntry != null;
        }

        public static List<EquipmentBase> GetRandomEquipment(int equipmentCount) {
            List<EquipmentBase> returnValue = new List<EquipmentBase>();

            for (int i = 0; i < equipmentCount; i++) {
                returnValue.Add(Instance.equipment.GetRandomElement());
            }

            return returnValue;
        }

        public static bool GetIDByName(string name, out int ID) {
            var dbEntry = Instance.equipment.FirstOrDefault(e => e.Name == name);
            ID = Instance.equipment.IndexOf(dbEntry);


            if (dbEntry == null) {
                Debug.Log($"Failed to find Equipment: {name} in database. Ensure it's been added");
            }

            return dbEntry != null;
        }

        public static bool TryGetByID(int ID, out EquipmentBase item) {
            if (ID >= Instance.equipment.Count) {
                item = null;
                Debug.Log($"Failed to find equipment by ID: {ID}");
                return false;
            }

            item = Instance.equipment.ElementAt(ID);
            return item != null;
        }
    }
}