using StarterAssets;
using System;
using UnityEngine;

public class PlatformMover : MonoBehaviour
{
    public MovePath path;
    public float speed = 5f;

    private int _currentWaypointIndex = 0;
    private float waitDuration = 2f;
    private float waitTimer = 0f;
    private bool moveBackwards = false;
    private PlatformTimeObject _timeObject;

    public int CurrentWayPointIndex { get { return _currentWaypointIndex; } set { _currentWaypointIndex = value; } }
    private float DeltaTime => _timeObject.ScaledDeltaTime;

    private void Awake()
    {
        _timeObject = GetComponent<PlatformTimeObject>();
    }

    private void Start()
    {
        transform.position = path.waypoints[0].position;
    }

    void Update()
    {
        if (waitTimer > 0f)
        {
            waitTimer -= DeltaTime;
            return;
        }

        var waypoints = path.waypoints;
        if (waypoints.Count == 0) return;

        // Move towards the current waypoint
        Transform targetWaypoint = waypoints[_currentWaypointIndex];
        Vector3 direction = targetWaypoint.position - transform.position;
        float distanceThisFrame = speed * DeltaTime;

        if (direction.magnitude <= distanceThisFrame)
        {
            // Snap to the waypoint and move to the next one
            transform.position = targetWaypoint.position;
            DetermineNextWayPoint();
        }
        else
        {
            // Move the platform
            transform.Translate(direction.normalized * distanceThisFrame, Space.World);
        }
    }

    private void DetermineNextWayPoint()
    {
        if (path.loop)
        {
            _currentWaypointIndex = (_currentWaypointIndex + 1) % path.waypoints.Count;
            waitTimer = waitDuration;
        }
        else if (moveBackwards)
        {
            CurrentWayPointIndex--;
            if (CurrentWayPointIndex < 0)
            {
                moveBackwards = false;
                CurrentWayPointIndex = 1;
                waitTimer = waitDuration;
            }
        }
        else
        {
            CurrentWayPointIndex++;
            if (CurrentWayPointIndex >= path.waypoints.Count)
            {
                moveBackwards = true;
                CurrentWayPointIndex = path.waypoints.Count - 2;
                waitTimer = waitDuration;
            }
        }
    }
}
