using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public enum RecorderState
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

    public static bool IntEquals(int a, int b)
    {
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
    public RecorderState state;
    public bool isRecording = false;
    public float aliveTime = 0f;


    [Space(10)]
    [Header("Physics Settings")]
    public int amountOfTimeZones = 0;
    public float currentTimeScale = 1;
    public float additionalTimeScale = 1;
    public float ScaledDeltaTime => currentTimeScale * Time.deltaTime * Time.timeScale;
    public float ScaledFixedDeltaTime => currentTimeScale * Time.fixedDeltaTime * Time.timeScale;

    public float gravityScale = 1f;
    protected const double TOLERANCE = 0.001f;
    public float defaultMass;
    public float initialTimeScale = 1f;
    public float frameDuration = 0.02f;

    protected virtual void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();

        state = RecorderState.None;
        _rigidbody.useGravity = false;
        defaultMass = _rigidbody.mass;
    }

    void FixedUpdate()
    {
        aliveTime += Time.fixedDeltaTime;
        if (_rigidbody.constraints == RigidbodyConstraints.FreezeAll) return;

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
            Log("TimeFreeze");
            if (state != RecorderState.Playback)
            {
                _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            }
        }
        else if (newTimeScale > TOLERANCE)
        {
            if (isTimeFrozen)
            {
                isTimeFrozen = false;
                Log("TimeUnfreeze");
                _rigidbody.constraints = RigidbodyConstraints.None;
            }

            if (state != RecorderState.Playback)
            {
                _rigidbody.mass = defaultMass / (newTimeScale * newTimeScale);
                _rigidbody.velocity = velocities.GetValueAtFrame(currentFrame) * relation;
                _rigidbody.angularVelocity = angularVelocities.GetValueAtFrame(currentFrame) * relation;
            }
        }
    }

    private void NormalizeTimeScale()
    {
        isTimeFrozen = false;
        _rigidbody.constraints = RigidbodyConstraints.None;
        _rigidbody.mass = defaultMass;

        float relation = currentTimeScale == 0f ? 1f : 1f / currentTimeScale;
        _rigidbody.velocity = velocities.GetValueAtFrame(currentFrame) * relation;
        _rigidbody.angularVelocity = angularVelocities.GetValueAtFrame(currentFrame) * relation;
        currentTimeScale = 1f;
    }

    private void FreezeRigidbody()
    {
        Log("Freeze");
        _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
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
        currentRecordingTime += ScaledFixedDeltaTime;
        var nextFrame = (int) Math.Floor(currentRecordingTime / frameDuration);
        Log($"Next Frame: {nextFrame}");
        Log($"Current Recording Time: {currentRecordingTime}");
        Log($"Current Fixed Delta Time: {ScaledFixedDeltaTime}");
        Log($"Current Time Scale: {currentTimeScale}");
        if (nextFrame <= currentFrame)
        {
            return;
        }
        currentFrame = nextFrame;
        Log($"Record Frame {currentFrame}: {transform.position}");
        positions.RecordIfChanged(currentFrame, transform.position, MathUtils.Vector3Equals);
        rotations.RecordIfChanged(currentFrame, transform.rotation, MathUtils.QuaternionEquals);
        velocities.RecordIfChanged(currentFrame, _rigidbody.velocity, MathUtils.Vector3Equals);
        angularVelocities.RecordIfChanged(currentFrame, _rigidbody.angularVelocity, MathUtils.Vector3Equals);
    }

    private void RecordInitialFrame()
    {
        currentFrame = 0;
        Log($"Record Initial Frame {currentFrame}: {transform.position}");
        positions.RecordIfChanged(currentFrame, transform.position, MathUtils.Vector3Equals);
        rotations.RecordIfChanged(currentFrame, transform.rotation, MathUtils.QuaternionEquals);
        velocities.RecordIfChanged(currentFrame, _rigidbody.velocity, MathUtils.Vector3Equals);
        angularVelocities.RecordIfChanged(currentFrame, _rigidbody.angularVelocity, MathUtils.Vector3Equals);
    }

    private void RecordFutureFramePosition(int frame, Vector3 position)
    {
        Log($"Record Specific Frame {frame}: {position}");
        positions.RecordIfChanged(frame, position, MathUtils.Vector3Equals);
        rotations.RecordIfChanged(frame, transform.rotation, MathUtils.QuaternionEquals);
        velocities.RecordIfChanged(frame, _rigidbody.velocity, MathUtils.Vector3Equals);
        angularVelocities.RecordIfChanged(frame, _rigidbody.angularVelocity, MathUtils.Vector3Equals);
    }


    private void Playback()
    {
        // Subtract the elapsed time.
        currentRecordingTime -= ScaledDeltaTime;

        // Calculate the total duration of the recording.
        // Wrap around the time if it goes below zero.
        if (currentRecordingTime < 0)
        {
            ChangeDirectionToRecord();
            currentRecordingTime = 0f;
            currentFrame = 0;
            return;
        }

        // Determine the frame indices using modulo arithmetic.
        currentFrame = (int)(currentRecordingTime / frameDuration);
        int nextFrame = currentFrame + 1;

        // Compute the interpolation factor between the two frames.
        float lerpFactor = (currentRecordingTime % frameDuration) / frameDuration;

        // Interpolate between the current and next positions.
        Vector3 currentPos = positions.GetValueAtFrame(currentFrame);
        Vector3 nextPos = positions.GetValueAtFrame(nextFrame);
        transform.position = Vector3.Lerp(currentPos, nextPos, lerpFactor);

        // Interpolate between the current and next rotations.
        Quaternion currentRot = rotations.GetValueAtFrame(currentFrame);
        Quaternion nextRot = rotations.GetValueAtFrame(nextFrame);
        transform.rotation = Quaternion.Lerp(currentRot, nextRot, lerpFactor);
    }

    private void Resume()
    {
    }

    public void OnFixedUpdate()
    {
        switch (state)
        {
            case RecorderState.Record:
                Record();
                break;

            default:
                break;
        }
    }

    public void OnUpdate()
    {
        switch (state)
        {
            case RecorderState.Playback:
                Playback();
                break;

            default:
                break;
        }
    }

    public void ChangeDirectionToRecord()
    {
        Log("Change Direction to Record");

        Log("Restore Initial Frame");
        transform.position = positions.GetValueAtFrame(0);
        transform.rotation = rotations.GetValueAtFrame(0);
        _rigidbody.velocity = velocities.GetValueAtFrame(0) * currentTimeScale;
        _rigidbody.angularVelocity = angularVelocities.GetValueAtFrame(0) * currentTimeScale;
        currentFrame = 0;

        state = RecorderState.Record;
        ClearAllExceptFirstFrame();

        Array.ForEach(renderers, r => r.material = resume);
        _rigidbody.constraints = isTimeFrozen ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
        _rigidbody.velocity = velocities.GetValueAtFrame(currentFrame) * currentTimeScale;
        _rigidbody.angularVelocity = angularVelocities.GetValueAtFrame(currentFrame) * currentTimeScale;
    }
    public void ChangeDirectionToPlayback()
    {
        Log("Change Direction to Playback");

        Array.ForEach(renderers, r => r.material = rewind);
        FreezeRigidbody();
        state = RecorderState.Playback;

        currentRecordingTime += ScaledFixedDeltaTime;
        var segmentDirection = transform.position - positions.GetValueAtFrame(currentFrame);
        var segmentDuration = currentRecordingTime % frameDuration;
        var segmentSpeed = segmentDirection / segmentDuration;
        var futureFramePosition = transform.position + segmentSpeed * frameDuration;
        RecordFutureFramePosition(currentFrame + 1, futureFramePosition);
        currentFrame++;
    }

    public void StartRecording(Vector3 centerPosition, float radius, float timeScale)
    {
        Log($"Start Recording {transform.position}");
        currentFrame = 0;
        currentRecordingTime = 0f;
        timeBubbleCenter = centerPosition;
        timeBubbleRadius = radius;
        isRecording = true;
        Array.ForEach(renderers, r => r.material = resume);

        RecordInitialFrame();
        state = RecorderState.Record;
        PitchTimeScale(timeScale);
    }

    public void StopRecording()
    {
        Log("Stop Recording");
        Resume();
        _rigidbody.constraints = RigidbodyConstraints.None;
        currentFrame = 0;
        state = RecorderState.None;
        isRecording = false;
        Array.ForEach(renderers, r => r.material = _default);
        positions.ClearAll();
        rotations.ClearAll();
        velocities.ClearAll();
        angularVelocities.ClearAll();
        NormalizeTimeScale();
    }

    private void Log(string v)
    {
        // Debug.Log($"{gameObject.name}: {v}");
    }

    public bool IsMovingForward()
    {
        return state == RecorderState.Record && currentFrame > 1;    
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
        Debug.DrawLine(transform.position, timeBubbleCenter, Color.red, 2f);
        Debug.DrawRay(timeBubbleCenter, (timeBubbleCenter - transform.position).normalized * timeBubbleRadius, Color.green, 2f);

        return MathUtils.IsPositionInsideOfSphere(transform.position, timeBubbleCenter, timeBubbleRadius);
    }
}
