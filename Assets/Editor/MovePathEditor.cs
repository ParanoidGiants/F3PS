using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MovePath))]
public class PathEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector fields first.
        DrawDefaultInspector();

        // Get reference to the MovePath script.
        MovePath path = (MovePath)target;

        GUILayout.Space(10);

        // Button to add a new waypoint.
        if (GUILayout.Button("Add Waypoint"))
        {
            Transform newWaypoint;

            // If there are already waypoints, position the new one slightly offset from the last one.
            if (path.waypoints.Count > 0)
            {
                Vector3 lastPos = path.waypoints[path.waypoints.Count - 1].position;
                newWaypoint = new GameObject("Waypoint " + path.waypoints.Count).transform;
                newWaypoint.position = lastPos + Vector3.right; // Adjust the offset as needed.
            }
            else
            {
                // If no waypoints exist, position it at the path's origin.
                newWaypoint = new GameObject("Waypoint 0").transform;
                newWaypoint.position = path.transform.position;
            }

            // Parent the new waypoint to the MovePath object.
            newWaypoint.parent = path.transform;
            path.waypoints.Add(newWaypoint);

            // Mark the scene as dirty to ensure the change is saved.
            EditorUtility.SetDirty(path);
        }

        // Button to remove the last waypoint.
        if (GUILayout.Button("Remove Last Waypoint"))
        {
            if (path.waypoints.Count > 0)
            {
                // Remove the last waypoint from the list.
                Transform lastWaypoint = path.waypoints[path.waypoints.Count - 1];
                path.waypoints.RemoveAt(path.waypoints.Count - 1);

                // Remove the waypoint from the scene.
                DestroyImmediate(lastWaypoint.gameObject);
            }
        }
    }

    // Draw handles for moving waypoints in the Scene view.
    private void OnSceneGUI()
    {
        MovePath path = (MovePath)target;
        for (int i = 0; i < path.waypoints.Count; i++)
        {
            if (path.waypoints[i] != null)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 newPos = Handles.PositionHandle(path.waypoints[i].position, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(path.waypoints[i], "Move Waypoint");
                    path.waypoints[i].position = newPos;
                }
            }
        }
    }
}
