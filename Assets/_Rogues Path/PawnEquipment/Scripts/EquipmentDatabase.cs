using System.Collections.Generic;
using System.Linq;
using _Rogues_Path._Game;
using _Rogues_Path.Utilities;
using HeroEditor.Common.Enums;
using UnityEngine;

namespace _Rogues_Path.Equipment.Scripts {
    [CreateAssetMenu(menuName = Game.Name + "/" + nameof(EquipmentDatabase), fileName = nameof(EquipmentDatabase))]
    public class EquipmentDatabase : ScriptableObject {
        #region Singleton
        private static EquipmentDatabase m_Instance;

        public static EquipmentDatabase Instance {
            get {
                if (m_Instance == null) {
                    m_Instance = Resources.Load<EquipmentDatabase>("Databases/EquipmentDatabase");
                }

                return m_Instance;
            }
        }
        #endregion


        public List<EquipmentBase> Equipment =>
            new List<EquipmentBase>(equipment);

        [SerializeField]
        private List<EquipmentBase> equipment = new();


        #region Lookup
        public static bool TryFind(EquipmentBase query, out EquipmentBase dbEntry) {

            dbEntry = null;

            if (query == null || Instance == null)
                return false;

            /*
             * If this already IS a database entry,
             * preserve the exact reference.
             */
            if (Instance.equipment.Contains(query)) {
                dbEntry = query;
                return true;
            }

            /*
             * Otherwise this is presumably a live clone.
             */
            dbEntry = Instance.equipment.FirstOrDefault(e => e != null && e.Name == query.Name);

            return dbEntry != null;
        }

        public static bool TryFindByName(string query, out EquipmentBase dbEntry) {

            dbEntry = null;

            if (Instance == null || string.IsNullOrEmpty(query)) {

                return false;
            }

            dbEntry = Instance.equipment.FirstOrDefault(e => e != null && e.Name == query);

            return dbEntry != null;
        }

        public static bool TryGetByID(int ID, out EquipmentBase item) {

            item = null;

            if (Instance == null)
                return false;

            if (ID < 0 || ID >= Instance.equipment.Count) {

                Debug.LogError($"Failed to find equipment by ID: {ID}");

                return false;
            }

            item = Instance.equipment[ID];

            return item != null;
        }

        public static bool TryGetID(EquipmentBase query, out int ID) {

            ID = -1;

            if (!TryFind(query, out EquipmentBase dbEntry))
                return false;

            ID = Instance.equipment.IndexOf(dbEntry);

            return ID >= 0;
        }

        public static bool GetIDByName(string name, out int ID) {

            ID = -1;

            if (!TryFindByName(name, out EquipmentBase dbEntry)) {

                Debug.LogError($"Failed to find Equipment: {name} in database. " + $"Ensure it's been added.");

                return false;
            }

            ID = Instance.equipment.IndexOf(dbEntry);

            return ID >= 0;
        }

        public static bool IsDatabaseEntry(EquipmentBase equipment) {

            return equipment != null && Instance != null && Instance.equipment.Contains(equipment);
        }
        #endregion


        #region Instantiation
        public static bool TryCreateInstance(int ID, out EquipmentBase instance, Transform parent = null) {

            instance = null;

            if (!TryGetByID(ID, out EquipmentBase template)) {

                return false;
            }

            return TryCreateInstanceFromTemplate(template, out instance, parent);
        }

        public static bool TryCreateInstance(EquipmentBase equipment, out EquipmentBase instance, Transform parent = null) {

            instance = null;

            if (!TryFind(equipment, out EquipmentBase template)) {

                Debug.LogError($"Failed to find {equipment?.Name} in EquipmentDatabase.");

                return false;
            }

            return TryCreateInstanceFromTemplate(template, out instance, parent);
        }

        private static bool TryCreateInstanceFromTemplate(EquipmentBase template, out EquipmentBase instance, Transform parent) {

            instance = null;

            if (template == null)
                return false;

            /*
             * Instantiate underneath an inactive staging GameObject.
             *
             * This prevents OnEnable() from firing while the instance
             * is being constructed, even if the template is active.
             */
            GameObject staging = new GameObject($"__{template.Name}_EquipmentStaging");

            staging.hideFlags = HideFlags.HideAndDontSave;

            staging.SetActive(false);

            instance = Instantiate(template, staging.transform);

            /*
             * Ensure the clone itself stays inactive when moved
             * underneath its real parent.
             */
            instance.gameObject.SetActive(false);

            instance.Owner = null;

            instance.transform.SetParent(parent, false);

            UnityEngine.Object.Destroy(staging);

            return true;
        }
        #endregion


        public static List<EquipmentBase> GetRandomEquipment(int equipmentCount) {

            List<EquipmentBase> returnValue = new();

            if (Instance == null)
                return returnValue;

            for (int i = 0; i < equipmentCount; i++) {
                returnValue.Add(Instance.equipment.GetRandomElement());
            }

            return returnValue;
        }
    }
}