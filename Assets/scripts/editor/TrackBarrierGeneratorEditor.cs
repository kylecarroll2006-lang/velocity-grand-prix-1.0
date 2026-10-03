using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TrackBarrierGenerator))]
public class TrackBarrierGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(15);

        TrackBarrierGenerator generator =
            (TrackBarrierGenerator)target;

        if (GUILayout.Button(
            "GENERATE BARRIERS",
            GUILayout.Height(40)))
        {
            generator.GenerateBarriers();
        }

        if (GUILayout.Button(
            "CLEAR BARRIERS",
            GUILayout.Height(30)))
        {
            generator.ClearBarriers();
        }
    }
}