using Assets.HeroEditor4D.Common.Scripts.Collections;
using UnityEditor;
using UnityEngine;

namespace Assets.HeroEditor4D.Common.Scripts.Editor
{
    /// <summary>
    /// Add "Refresh" button to SpriteCollection script
    /// </summary>
    [CustomEditor(typeof(SpriteCollection))]
    public class SpriteCollectionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var spriteCollection = (SpriteCollection) target;

            if (GUILayout.Button("Refresh"))
            {
	            Debug.ClearDeveloperConsole();
				SpriteCollectionRefresh.Refresh(spriteCollection);
            }
        }
    }
}