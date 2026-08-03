using System.Linq;
using Assets.HeroEditor.Common.Scripts.Collections;
using UnityEditor;
using UnityEngine;

namespace Assets.HeroEditor.Common.Scripts.Editor
{
    /// <summary>
    /// Refreshes the main icon collection when importing new sprite bundles.
    /// </summary>
    public class IconCollectionRefresh : AssetPostprocessor
    {
        private static IconCollection[] _collections;

        public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            var assets = importedAssets.Union(deletedAssets).Union(movedAssets).ToList();

            if (assets.All(i => !i.Contains("/Icons/"))) return;

            _collections ??= Resources.LoadAll<IconCollection>("");

            foreach (var collection in _collections)
            {
                var folders = collection.IconFolders.Select(AssetDatabase.GetAssetPath).ToList();

                foreach (var folder in folders)
                {
                    if (assets.Any(i => i.StartsWith(folder)))
                    {
                        collection.Refresh();
                        break;
                    }
                }
            }
        }
    }
}