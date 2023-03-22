using UnityEditor;
using UnityEngine;
using System.Collections;


[CustomEditor(typeof(WaypointPath))]
public class WaypointPathEditor : Editor
{

    Editor currentTransformEditor;
    Transform[] waypoints;
    Transform selectedTransform;
    string[] optionsList;
    int index = 0;
    WaypointPath myWayPath;

    void GetWaypoints()
    {
        myWayPath = target as WaypointPath;

        if (myWayPath.wayPointArray != null)
        {
            optionsList = new string[myWayPath.wayPointArray.Length];

            for (int i = 0; i < optionsList.Length; i++)
            {
                optionsList[i] = myWayPath.wayPointArray[i].name;
            }
        }
    }

    public override void OnInspectorGUI()
    {
        GetWaypoints();
        DrawDefaultInspector();
        EditorGUILayout.Space();
        EditorGUI.BeginChangeCheck();

        if (optionsList != null)
            index = EditorGUILayout.Popup("Select Waypoint", index, optionsList);

        if (EditorGUI.EndChangeCheck())
        {
            Editor tmpEditor = null;

            if (index < myWayPath.wayPointArray.Length)
            {
                selectedTransform = myWayPath.wayPointArray[index];

                //Creates an Editor for selected Component from a Popup
                tmpEditor = Editor.CreateEditor(selectedTransform);
            }
            else
            {
                selectedTransform = null;
            }

            // If there isn't a Transform currently selected then destroy the existing editor
            if (currentTransformEditor != null)
            {
                DestroyImmediate(currentTransformEditor);
            }

            currentTransformEditor = tmpEditor;
        }

        // Shows the created Editor beneath CustomEditor
        if (currentTransformEditor != null && selectedTransform != null)
        {
            currentTransformEditor.OnInspectorGUI();
        }
    }
}