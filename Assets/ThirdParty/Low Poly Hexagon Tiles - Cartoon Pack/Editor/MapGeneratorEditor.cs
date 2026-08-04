using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MapGenerator myScript = (MapGenerator)target;

        if (GUILayout.Button("Generate Field"))
        {
            myScript.GenerateField();
        }
        if (GUILayout.Button("Delete Field"))
        {
            myScript.DeleteField();
        }
    }
}
