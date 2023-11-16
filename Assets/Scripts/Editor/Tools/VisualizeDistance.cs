using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// This is a tool to tell how far two objects in game space are. It will help with getting distances between order dropoffs and pickups.
/// </summary>
public class VisualizeDistance : EditorWindow
{
    private GameObject pickup, dropoff;

    [MenuItem("Tools/Calculate Distance")]

    public static void ShowWindow()
    {
        GetWindow<VisualizeDistance>("Calculate Distance");
    }

    /// <summary>
    /// Calls whenever the window is opened.
    /// </summary>
    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Pickup", GUILayout.MaxWidth(100));

        if(GUILayout.Button("Pickup"))
        {
            pickup = Selection.activeGameObject;
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        
        EditorGUILayout.LabelField("Dropoff", GUILayout.MaxWidth(100));

        if (GUILayout.Button("Dropoff"))
        {
            dropoff = Selection.activeGameObject;
        }

        GUILayout.EndHorizontal();

        if(pickup != null && dropoff != null) 
        {
            float distance = Vector3.Distance(pickup.transform.position, dropoff.transform.position);
            GUILayout.Label($"Distance Betweeen {pickup.name} and {dropoff.name} is {distance:F2} units.");
        }
    }
}
