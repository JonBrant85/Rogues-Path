using Assets.FantasyInventoryIcons.Scripts;
using UnityEditor;
using UnityEngine;

namespace Assets.FantasyInventoryIcons.Editor
{
    [CustomEditor(typeof(IconManager))]
    public class PrefabManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var script = (IconManager) target;

            if (GUILayout.Button("Refresh"))
            {
                script.Refresh();
            }
        }
    }
}