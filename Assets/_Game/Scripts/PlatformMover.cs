using System;
using UnityEngine;


public class PlatformMover : MonoBehaviour
{
    public MovePath path;
    public float speed = 5f;
    public float waitDuration = 2f;
    public int CurrentWaypointIndex = 0;
    private float waitTimer = 0f;
    private bool moveBackwards = false;
    private Rigidbody _rigidbody;
    private PlatformTimeObject _timeObject;
    private float FixedDeltaTime => _timeObject != null ? _timeObject.ScaledFixedDeltaTime : Time.fixedDeltaTime;

    public Transform fromWaypoint;
    public Transform toWaypoint;
    public float translationTime = 0f;
    public float timeBetweenCurrentWaypoints;

    private void Awake()
    {
        _timeObject = GetComponent<PlatformTimeObject>();
        _rigidbody = GetComponent<Rigidbody>();
        if (_rigidbody == null)
        {
            Debug.LogError("PlatformMover requires a Rigidbody component.", this);
        }
    }

    private void Start()
    {
        _rigidbody.position = path.waypoints[0].position;
        CurrentWaypointIndex = 1;
        SetupPathFromTo(0, CurrentWaypointIndex);
    }

    private void SetupPathFromTo(int from, int to)
    {
        fromWaypoint = path.waypoints[from];
        toWaypoint = path.waypoints[to];
        timeBetweenCurrentWaypoints = Vector3.Distance(fromWaypoint.position, toWaypoint.position) / speed;
        translationTime = 0f;
    }

    void FixedUpdate()
    {
        if (waitTimer > 0f)
        {
            waitTimer -= FixedDeltaTime;
            return;
        }

        var waypoints = path.waypoints;
        if (waypoints.Count == 0) return;


        if (translationTime >= timeBetweenCurrentWaypoints)
        {
            _rigidbody.MovePosition(toWaypoint.position);
            _rigidbody.MoveRotation(toWaypoint.rotation);
            DetermineNextWayPoint();
        }

        var currentPosition = Vector3.Lerp(fromWaypoint.position, toWaypoint.position, translationTime / timeBetweenCurrentWaypoints);
        var currentRotation = Quaternion.Slerp(fromWaypoint.rotation, toWaypoint.rotation, translationTime / timeBetweenCurrentWaypoints);
        _rigidbody.MovePosition(currentPosition);
        _rigidbody.MoveRotation(currentRotation);
        translationTime += FixedDeltaTime;
    }

    private void DetermineNextWayPoint()
    {
        var oldWaypointIndex = CurrentWaypointIndex;
        if (path.loop)
        {
            CurrentWaypointIndex = (CurrentWaypointIndex + 1) % path.waypoints.Count;
            waitTimer = waitDuration;
        }
        else if (moveBackwards)
        {
            CurrentWaypointIndex--;
            if (CurrentWaypointIndex < 0)
            {
                moveBackwards = false;
                CurrentWaypointIndex = 1;
                waitTimer = waitDuration;
            }
        }
        else
        {
            CurrentWaypointIndex++;
            if (CurrentWaypointIndex >= path.waypoints.Count)
            {
                moveBackwards = true;
                CurrentWaypointIndex = path.waypoints.Count - 2;
                waitTimer = waitDuration;
            }
        }
        SetupPathFromTo(oldWaypointIndex, CurrentWaypointIndex);
        fromWaypoint = path.waypoints[oldWaypointIndex];
        toWaypoint = path.waypoints[CurrentWaypointIndex];
        timeBetweenCurrentWaypoints = Vector3.Distance(fromWaypoint.position, toWaypoint.position) / speed;
        translationTime = 0f;
    }
}