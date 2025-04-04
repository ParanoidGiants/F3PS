using UnityEngine;
using System.Collections.Generic;

public enum PathType
{
    Linear,
    Circular
}

[ExecuteAlways]
public class MovePath : MonoBehaviour
{
    public List<Transform> waypoints = new List<Transform>();
    public PathType pathType = PathType.Linear;

    private void OnDrawGizmos()
    {
        if (waypoints.Count < 2)
            return;

        if (pathType == PathType.Circular)
        {
            DrawCircularPath();
        }
        else if (pathType == PathType.Linear)
        {
            DrawLinearPath();
        }
    }

    private void DrawCircularPath()
    {
        Gizmos.color = Color.blue;
        for (int i = 0; i < waypoints.Count; i++)
        {
            var nextIndex = (i + 1) % waypoints.Count;
            if (waypoints[i] != null && waypoints[nextIndex] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[nextIndex].position);
            }
        }
    }

    private void DrawLinearPath()
    {
        Gizmos.color = Color.green;
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }
    }

    private void OnValidate()
    {
        // Ensure all waypoints are unique
        HashSet<Transform> uniqueWaypoints = new HashSet<Transform>(waypoints);
        if (uniqueWaypoints.Count != waypoints.Count)
        {
            Debug.LogWarning("Duplicate waypoints found. Please ensure all waypoints are unique.");
            waypoints = new List<Transform>(uniqueWaypoints);
        }
    }
}
