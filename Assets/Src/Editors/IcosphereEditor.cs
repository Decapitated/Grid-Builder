using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Icosphere))]
public class IcosphereEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Icosphere icosphere = (Icosphere)target;
        EditorGUILayout.HelpBox("This is a help box", MessageType.Info);

        if (GUILayout.Button("Generate")) icosphere.Generate();
    }
}
