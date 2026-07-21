using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.FantasyInventoryIcons.Scripts
{
    public class IconManager : MonoBehaviour
    {
        public Object IconFolder;
        public List<Sprite> Icons;
        public Transform Grid;
        public GameObject ItemPrefab;
        public List<Sprite> Backgrounds;

        #if UNITY_EDITOR
        public void Refresh()
        {
            if (IconFolder == null) return;
        
            var path = AssetDatabase.GetAssetPath(IconFolder);
            var files = Directory.GetFiles(path, "*.png", SearchOption.AllDirectories);

            Icons = files.Select(AssetDatabase.LoadAssetAtPath<Sprite>).ToList();

            var icons = new List<GameObject>();

            for (var i = 0; i < Grid.childCount; i++)
            {
                icons.Add(Grid.GetChild(i).gameObject);
            }

            icons.ForEach(DestroyImmediate);

            foreach (var icon in Icons)
            {
                var instance = Instantiate(ItemPrefab, Grid);

                instance.transform.Find("Icon").GetComponent<Image>().sprite = icon;
                instance.transform.Find("Background").GetComponent<Image>().sprite = Backgrounds[Random.Range(0, Backgrounds.Count)];
            }
        }
        #endif
    }
}
