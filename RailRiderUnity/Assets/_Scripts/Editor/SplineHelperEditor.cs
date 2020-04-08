using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SplineHelper))]
public class SplineHelperEditor : Editor
{
    // Update is called once per frame
    private void OnEnable()
    {

    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SplineHelper sh = (SplineHelper)target;

        if (GUILayout.Button("Update Nodes"))
        {
            sh.UpdateNodeData();
        }
    }    
}
