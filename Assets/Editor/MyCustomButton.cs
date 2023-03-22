using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Mesh), true)]
[CanEditMultipleObjects]
public class CustomMeshInspector : Editor
{
    //Unity's built-in editor
    Editor defaultEditor;
    Mesh mesh;

    void OnEnable()
    {
        //When this inspector is created, also create the built-in inspector
        defaultEditor = Editor.CreateEditor(targets, Type.GetType("UnityEditor.ModelInspector, UnityEditor"));
        mesh = target as Mesh;

    }

    void OnDisable()
    {
        //When OnDisable is called, the default editor we created should be destroyed to avoid memory leakage.
        //Also, make sure to call any required methods like OnDisable
        MethodInfo disableMethod = defaultEditor.GetType().GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (disableMethod != null)
            disableMethod.Invoke(defaultEditor, null);
        DestroyImmediate(defaultEditor);
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("Optimization Settings", EditorStyles.boldLabel);
        if (GUILayout.Button("Reduce Mesh"))
        {
            Debug.Log("REDUCING MESH! ");
        }
        defaultEditor.OnInspectorGUI();
    }
}