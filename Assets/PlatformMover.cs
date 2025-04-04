using StarterAssets;
using UnityEngine;

public class PlatformMover : MonoBehaviour
{
    public MovePath path;  // Assign these in the Inspector
    public float speed = 5f;
    private int _currentWaypointIndex = 0;
    private PlatformRecorder recorder;

    public int CurrentWayPointIndex { get { return _currentWaypointIndex; } set { _currentWaypointIndex = value; } }

    private void Start()
    {
        recorder = GetComponent<PlatformRecorder>();
        transform.position = path.waypoints[0].position;
    }

    void Update()
    {
        if (recorder.isFrozen || recorder.state == RecorderState.Playback)
        {
            return;
        }

        var waypoints = path.waypoints;
        if (waypoints.Count == 0) return;

        // Move towards the current waypoint
        Transform targetWaypoint = waypoints[_currentWaypointIndex];
        Vector3 direction = targetWaypoint.position - transform.position;
        float distanceThisFrame = speed * recorder.ScaledDeltaTime;

        if (direction.magnitude <= distanceThisFrame)
        {
            // Snap to the waypoint and move to the next one
            transform.position = targetWaypoint.position;
            _currentWaypointIndex = (_currentWaypointIndex + 1) % waypoints.Count;
        }
        else
        {
            // Move the platform
            transform.Translate(direction.normalized * distanceThisFrame, Space.World);
        }
    }
}
