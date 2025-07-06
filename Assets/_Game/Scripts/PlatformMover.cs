using System;
using UnityEngine;


public class PlatformMover : MonoBehaviour
{
    [Header("Path Reference")]
    public MovePath path;

    [Space(10)]
    [Header("Movement Settings")]
    public float movementSpeed = 5f;
    [Tooltip("This is only used when the from and to waypoints have the same position")]
    public float rotationSpeed = 5f;
    public float waitDuration = 2f;
    public int currentWaypointIndex = 0;


    private float waitTimer = 0f;
    private bool moveBackwards = false;
    private Rigidbody _rigidbody;
    private PlatformTimeObject _timeObject;
    private float FixedDeltaTime => _timeObject != null ? _timeObject.ScaledFixedDeltaTime : Time.fixedDeltaTime;

    public Transform fromWaypoint;
    public Transform toWaypoint;
    public float translationBetweenWaypointsTime = 0f;
    public float translationBetweenWayPointsDuration;

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
        currentWaypointIndex = 1;
        SetupPathFromTo(0, currentWaypointIndex);
    }

    private void SetupPathFromTo(int from, int to)
    {
        fromWaypoint = path.waypoints[from];
        toWaypoint = path.waypoints[to];
        translationBetweenWayPointsDuration = Vector3.Distance(fromWaypoint.position, toWaypoint.position) / movementSpeed;
        translationBetweenWaypointsTime = 0f;
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


        if (translationBetweenWaypointsTime >= translationBetweenWayPointsDuration)
        {
            _rigidbody.MovePosition(toWaypoint.position);
            _rigidbody.MoveRotation(toWaypoint.rotation);
            DetermineNextWayPoint();
        }

        var currentPosition = Vector3.Lerp(fromWaypoint.position, toWaypoint.position, translationBetweenWaypointsTime / translationBetweenWayPointsDuration);
        _rigidbody.MovePosition(currentPosition);

        var currentRotation = Quaternion.Slerp(fromWaypoint.rotation, toWaypoint.rotation, translationBetweenWaypointsTime / translationBetweenWayPointsDuration);
        _rigidbody.MoveRotation(currentRotation);

        translationBetweenWaypointsTime += FixedDeltaTime;
    }

    private void DetermineNextWayPoint()
    {
        var oldWaypointIndex = currentWaypointIndex;
        if (path.loop)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % path.waypoints.Count;
            waitTimer = waitDuration;
        }
        else if (moveBackwards)
        {
            currentWaypointIndex--;
            if (currentWaypointIndex < 0)
            {
                moveBackwards = false;
                currentWaypointIndex = 1;
                waitTimer = waitDuration;
            }
        }
        else
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= path.waypoints.Count)
            {
                moveBackwards = true;
                currentWaypointIndex = path.waypoints.Count - 2;
                waitTimer = waitDuration;
            }
        }
        SetupPathFromTo(oldWaypointIndex, currentWaypointIndex);
        fromWaypoint = path.waypoints[oldWaypointIndex];
        toWaypoint = path.waypoints[currentWaypointIndex];
        translationBetweenWayPointsDuration = Vector3.Distance(fromWaypoint.position, toWaypoint.position) / movementSpeed;
        if (translationBetweenWayPointsDuration <= 0f)
        {
            Debug.LogWarning("Translation duration is zero or negative, using rotation speed to determine duration.", this);
            translationBetweenWayPointsDuration = Quaternion.Angle(fromWaypoint.rotation, toWaypoint.rotation) / rotationSpeed;
        }
        if (translationBetweenWayPointsDuration <= 0f)
        {
            Debug.LogError("The position and rotation of the two waypoints is equal.");
            Debug.LogError("Please ensure that the from and to waypoints are not the same.");
        }

        translationBetweenWaypointsTime = 0f;
    }
}