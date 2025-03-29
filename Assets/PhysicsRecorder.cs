using System;
using UnityEngine;

public enum PhysicsRecorderState
{
    None,
    Record,
    Playback
}

public static class MathUtils
{
    public static bool IsPositionInsideOfSphere(Vector3 position, Vector3 center, float radius)
    {
        return Vector3.Distance(position, center) <= radius;
    }


    public static Vector3? GetSphereIntersectionPoint(Vector3 center, float radius, Vector3 point, Vector3 direction)
    {
        // Normalize the direction to ensure accurate calculations.
        direction = direction.normalized;

        // Compute the vector from the sphere's center to the ray's origin.
        Vector3 m = point - center;

        // Coefficient for the linear term.
        float b = Vector3.Dot(m, direction);
        // The constant term.
        float c = Vector3.Dot(m, m) - radius * radius;

        // If point is outside the sphere and the ray is pointing away from the sphere, no intersection.
        if (c > 0f && b > 0f)
        {
            return null;
        }

        // Calculate the discriminant of the quadratic equation.
        float discriminant = b * b - c;
        if (discriminant < 0f)
        {
            // No real roots, so the line does not intersect the sphere.
            return null;
        }

        // Compute the smallest t value (the nearest intersection point along the ray).
        float t = -b - Mathf.Sqrt(discriminant);

        // If t is negative, it means the ray started inside the sphere,
        // so we take the other intersection (the exit point).
        if (t < 0f)
        {
            t = -b + Mathf.Sqrt(discriminant);
        }

        // Return the intersection point.
        return point + t * direction;
    }

    public static bool Vector3Equals(Vector3 a, Vector3 b)
    {
        // Using the overloaded operator for Vector3.
        return a == b;
    }

    public static bool QuaternionEquals(Quaternion a, Quaternion b)
    {
        // Using the overloaded operator for Quaternion.
        return a == b;
    }
}


public class PhysicsRecorder : MonoBehaviour
{
    private Rigidbody _rigidbody;

    public PropertyRecorder<Vector3> positions = new();
    public PropertyRecorder<Quaternion> rotations = new();
    public PropertyRecorder<Vector3> velocities = new();
    public PropertyRecorder<Vector3> angularVelocities = new();

    [Header("Playback")]
    public int playbackSpeed = 0;

    [Space(20)]
    [Header("Debug")]
    public Renderer[] renderers;
    public Material _default;
    public Material resume;
    public Material rewind;
    public Material floating;

    [Space(20)]
    [Header("Watchers")]
    public float currentRecordingTime = 0;
    public int currentFrame = 0;
    public Vector3 timeBubbleCenter;
    public float timeBubbleRadius;
    public PhysicsRecorderState state;
    public bool isRecording = false;
    public float aliveTime = 0f;


    [Space(10)]
    [Header("Physics Settings")]
    public int amountOfTimeZones = 0;
    public float currentTimeScale = 1;
    public float additionalTimeScale = 1;
    public float ScaledDeltaTime => currentTimeScale * Time.deltaTime;
    public float gravityScale = 1f;
    protected const double TOLERANCE = 0.001f;
    public float defaultMass;
    public float initialTimeScale = 1f;
    public bool isFrozen = false;

    protected virtual void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();

        state = PhysicsRecorderState.None;
        _rigidbody.useGravity = false;
        defaultMass = _rigidbody.mass;
    }

    void FixedUpdate()
    {
        aliveTime += Time.fixedDeltaTime;
        if (_rigidbody.isKinematic) return;

        var force = Physics.gravity * (currentTimeScale * currentTimeScale * gravityScale);
        _rigidbody.AddForce(
            force,
            ForceMode.Acceleration
        );
    }

    private void OnEnable()
    {
        aliveTime = 0f;
    }

    public bool isTimeFrozen = false;
    public void PitchTimeScale(float newTimeScale)
    {
        float relation = currentTimeScale == 0f ? 1f : newTimeScale / currentTimeScale;
        currentTimeScale = newTimeScale;

        if (!isTimeFrozen && newTimeScale <= TOLERANCE)
        {
            isTimeFrozen = true;
            Debug.Log("TimeFreeze");
            if (state != PhysicsRecorderState.Playback)
            {
                _rigidbody.isKinematic = true;
                _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            }
        }
        else if (newTimeScale > TOLERANCE)
        {
            if (isTimeFrozen)
            {
                isTimeFrozen = false;
                Debug.Log("TimeUnfreeze");
                _rigidbody.isKinematic = false;
                _rigidbody.constraints = RigidbodyConstraints.None;
            }

            if (state != PhysicsRecorderState.Playback)
            {
                _rigidbody.mass = defaultMass / (newTimeScale * newTimeScale);
                _rigidbody.velocity = velocities.GetValueAtFrame(currentFrame) * relation;
                _rigidbody.angularVelocity = angularVelocities.GetValueAtFrame(currentFrame) * relation;
            }
        }
    }

    private void FreezeRigidbody()
    {
        Debug.Log("Freeze");
        _rigidbody.isKinematic = true;
        _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
    }

    private void UnfreezeRigidbody()
    {
        if (state == PhysicsRecorderState.Playback)
        {
            return;
        }
        _rigidbody.isKinematic = false;
        _rigidbody.constraints = RigidbodyConstraints.None;
        _rigidbody.velocity = velocities.GetValueAtFrame(currentFrame) * currentTimeScale;
        _rigidbody.angularVelocity = angularVelocities.GetValueAtFrame(currentFrame) * currentTimeScale;
    }

    private void OnDisable()
    {
        if (isRecording)
        {
            StopRecording();
        }
    }

    private void Record()
    {
        currentRecordingTime += Time.fixedDeltaTime * currentTimeScale * Time.timeScale;
        var nextFrame = (int) Math.Floor(currentRecordingTime / Time.fixedDeltaTime);
        if (nextFrame <= currentFrame)
        {
            return;
        }
        currentFrame = nextFrame;
        Debug.Log($"Record Frame {currentFrame}: {transform.position}");
        positions.RecordIfChanged(currentFrame, transform.position, MathUtils.Vector3Equals);
        rotations.RecordIfChanged(currentFrame, transform.rotation, MathUtils.QuaternionEquals);
        velocities.RecordIfChanged(currentFrame, _rigidbody.velocity, MathUtils.Vector3Equals);
        angularVelocities.RecordIfChanged(currentFrame, _rigidbody.angularVelocity, MathUtils.Vector3Equals);
    }

    private void RecordInitialFrame()
    {
        currentFrame = 0;
        Debug.Log($"Record Initial Frame {currentFrame}: {transform.position}");
        positions.RecordIfChanged(currentFrame, transform.position, MathUtils.Vector3Equals);
        rotations.RecordIfChanged(currentFrame, transform.rotation, MathUtils.QuaternionEquals);
        velocities.RecordIfChanged(currentFrame, _rigidbody.velocity, MathUtils.Vector3Equals);
        angularVelocities.RecordIfChanged(currentFrame, _rigidbody.angularVelocity, MathUtils.Vector3Equals);
    }

    private void RecordFutureFramePosition(int frame, Vector3 position)
    {
        Debug.Log($"Record Specific Frame {frame}: {position}");
        positions.RecordIfChanged(frame, position, MathUtils.Vector3Equals);
        rotations.RecordIfChanged(frame, transform.rotation, MathUtils.QuaternionEquals);
        velocities.RecordIfChanged(frame, _rigidbody.velocity, MathUtils.Vector3Equals);
        angularVelocities.RecordIfChanged(frame, _rigidbody.angularVelocity, MathUtils.Vector3Equals);
    }

    private void Playback()
    {

        currentRecordingTime -= Time.fixedDeltaTime * currentTimeScale * Time.timeScale;
        int nextFrame = (int)Math.Floor(currentRecordingTime / Time.fixedDeltaTime);

        if (nextFrame + 1 >= currentFrame)
        {
            var lerpPosition = (currentRecordingTime % Time.fixedDeltaTime) / Time.fixedDeltaTime;
            var nextPosition = positions.GetValueAtFrame(nextFrame);
            var currentPosition = positions.GetValueAtFrame(currentFrame);
            transform.position = Vector3.Lerp(nextPosition, currentPosition, lerpPosition);

            var nextRotation = rotations.GetValueAtFrame(nextFrame);
            var currentRotation = rotations.GetValueAtFrame(currentFrame);
            transform.rotation = Quaternion.Lerp(nextRotation, currentRotation, lerpPosition);
            return;
        }
        currentFrame--;

        if (currentRecordingTime < 0)
        {
            ChangeDirectionToRecord();
            currentRecordingTime = 0f;
            currentFrame = 0;
            return;
        }
    }

    private void Resume()
    {
        Array.ForEach(renderers, r => r.material = resume);

        UnfreezeRigidbody();

        _rigidbody.velocity = velocities.GetValueAtFrame(currentFrame) * currentTimeScale;
        _rigidbody.angularVelocity = angularVelocities.GetValueAtFrame(currentFrame) * currentTimeScale;
    }

    public void OnFixedUpdate()
    {
        switch (state)
        {
            case PhysicsRecorderState.None:
                break;

            case PhysicsRecorderState.Record:
                Record();
                break;

            case PhysicsRecorderState.Playback:
                Playback();
                break;

            default:
                break;
        }
    }

    public void ChangeDirectionToRecord()
    {
        Debug.Log("Change Direction to Record");

        Debug.Log("Restore Initial Frame");
        transform.position = positions.GetValueAtFrame(0);
        transform.rotation = rotations.GetValueAtFrame(0);
        _rigidbody.velocity = velocities.GetValueAtFrame(0) * currentTimeScale;
        _rigidbody.angularVelocity = angularVelocities.GetValueAtFrame(0) * currentTimeScale;
        currentFrame = 0;

        state = PhysicsRecorderState.Record;
        ClearAllExceptFirstFrame();
        Resume();
    }
    public void ChangeDirectionToPlayback()
    {
        Debug.Log("Change Direction to Playback");

        Array.ForEach(renderers, r => r.material = rewind);
        FreezeRigidbody();
        state = PhysicsRecorderState.Playback;

        currentRecordingTime += Time.fixedDeltaTime * currentTimeScale * Time.timeScale;
        var segmentDirection = transform.position - positions.GetValueAtFrame(currentFrame);
        var segmentDuration = currentRecordingTime % Time.fixedDeltaTime;
        var segmentSpeed = segmentDirection / segmentDuration;
        var futureFramePosition = transform.position + segmentSpeed * Time.fixedDeltaTime;
        RecordFutureFramePosition(currentFrame + 1, futureFramePosition);
        currentFrame++;
    }

    public void StartRecording(Vector3 centerPosition, float radius)
    {
        Debug.Log("Start Recording");
        timeBubbleCenter = centerPosition;
        timeBubbleRadius = radius;
        isRecording = true;
        Array.ForEach(renderers, r => r.material = resume);

        if (aliveTime > Time.fixedDeltaTime)
        {
            var initialPoint = MathUtils.GetSphereIntersectionPoint(centerPosition, radius, transform.position, -_rigidbody.velocity);
            if (initialPoint != null)
            {
                transform.position = (Vector3)initialPoint;
            }
        }
        RecordInitialFrame();
        state = PhysicsRecorderState.Record;
    }

    public void StopRecording()
    {
        Debug.Log("Stop Recording");
        Resume();
        currentFrame = 0;
        state = PhysicsRecorderState.None;
        isRecording = false;
        Array.ForEach(renderers, r => r.material = _default);
        ClearAll();
        PitchTimeScale(1f);
    }

    public bool IsMovingForward()
    {
        return state == PhysicsRecorderState.Record && currentFrame > 1;    
    }

    private void ClearAll()
    {
        positions.ClearAll();
        rotations.ClearAll();
        velocities.ClearAll();
        angularVelocities.ClearAll();
    }

    private void ClearAllExceptFirstFrame()
    {
        positions.ClearAllExceptFirstFrame();
        rotations.ClearAllExceptFirstFrame();
        velocities.ClearAllExceptFirstFrame();
        angularVelocities.ClearAllExceptFirstFrame();
    }

    public bool IsInSphere()
    {
        return MathUtils.IsPositionInsideOfSphere(transform.position, timeBubbleCenter, timeBubbleRadius);
    }
}
