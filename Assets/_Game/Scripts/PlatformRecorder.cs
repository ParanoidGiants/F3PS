using System;
using UnityEngine;

public class PlatformRecorder : MonoBehaviour
{
    public PropertyRecorder<Vector3> positions = new();
    public PropertyRecorder<Quaternion> rotations = new();
    public PropertyRecorder<int> wayPoints = new();

    [Header("References")]
    public PlatformMover platformMover;

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
    public Vector3 khonsuSphereCenter;
    public float khonsuSphereRadius;
    public RecorderState state;
    public bool isRecording = false;
    public float aliveTime = 0f;


    [Space(10)]
    [Header("Settings")]
    public float currentTimeScale = 1;
    public float ScaledDeltaTime => currentTimeScale * Time.deltaTime * Time.timeScale;
    protected const double TOLERANCE = 0.001f;
    public float initialTimeScale = 1f;
    public bool isFrozen = false;
    public float frameDuration = 0.02f;

    protected virtual void Awake()
    {
        state = RecorderState.None;
    }

    void Update()
    {
        aliveTime += ScaledDeltaTime;
    }

    private void OnEnable()
    {
        aliveTime = 0f;
    }

    public bool isTimeFrozen = false;
    public void PitchTimeScale(float newTimeScale)
    {
        currentTimeScale = newTimeScale;

        if (!isTimeFrozen && newTimeScale <= TOLERANCE)
        {
            isTimeFrozen = true;
        }
        else if (isTimeFrozen && newTimeScale > TOLERANCE)
        {
            isTimeFrozen = false;
        }
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
        currentRecordingTime += ScaledDeltaTime;
        var nextFrame = (int)Math.Floor(currentRecordingTime / frameDuration);
        Log($"Next Frame: {nextFrame}");
        Log($"Current Recording Time: {currentRecordingTime}");
        Log($"Current Time Scale: {currentTimeScale}");
        if (nextFrame <= currentFrame)
        {
            return;
        }
        currentFrame = nextFrame;
        Log($"Record Frame {currentFrame}: {transform.position}");
        positions.RecordIfChanged(currentFrame, transform.position, MathUtils.Vector3Equals);
        rotations.RecordIfChanged(currentFrame, transform.rotation, MathUtils.QuaternionEquals);
        wayPoints.RecordIfChanged(currentFrame, platformMover.CurrentWayPointIndex, MathUtils.IntEquals);
    }

    private void RecordInitialFrame()
    {
        currentFrame = 0;
        Log($"Record Initial Frame {currentFrame}: {transform.position}");
        positions.RecordIfChanged(currentFrame, transform.position, MathUtils.Vector3Equals);
        rotations.RecordIfChanged(currentFrame, transform.rotation, MathUtils.QuaternionEquals);
        wayPoints.RecordIfChanged(currentFrame, platformMover.CurrentWayPointIndex, MathUtils.IntEquals);
    }

    private void RecordFutureFramePosition(int frame, Vector3 position)
    {
        Log($"Record Specific Frame {frame}: {position}");
        positions.RecordIfChanged(frame, position, MathUtils.Vector3Equals);
        rotations.RecordIfChanged(frame, transform.rotation, MathUtils.QuaternionEquals);
    }
    private void Playback()
    {
        // Subtract the elapsed time.
        currentRecordingTime -= ScaledDeltaTime;

        // Calculate the total duration of the recording.
        float totalRecordingTime = positions.Count * frameDuration;

        // Wrap around the time if it goes below zero.
        if (currentRecordingTime < 0)
        {
            ChangeDirectionToRecord();
            currentRecordingTime = 0f;
            currentFrame = 0;
            return;
        }

        // Determine the frame indices using modulo arithmetic.
        int frameCount = positions.Count;
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
        Array.ForEach(renderers, r => r.material = resume);
    }

    public void OnUpdate()
    {
        switch (state)
        {
            case RecorderState.None:
                break;

            case RecorderState.Record:
                Record();
                break;

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
        platformMover.CurrentWayPointIndex = wayPoints.GetValueAtFrame(0);
        currentFrame = 0;

        state = RecorderState.Record;
        positions.ClearAllExceptFirstFrame();
        rotations.ClearAllExceptFirstFrame();
        wayPoints.ClearAllExceptFirstFrame();
        Resume();
    }
    public void ChangeDirectionToPlayback()
    {
        Log("Change Direction to Playback");

        Array.ForEach(renderers, r => r.material = rewind);
        state = RecorderState.Playback;

        currentRecordingTime += ScaledDeltaTime;
        var segmentDirection = transform.position - positions.GetValueAtFrame(currentFrame);
        var segmentDuration = currentRecordingTime % ScaledDeltaTime;
        var segmentSpeed = segmentDirection / segmentDuration;
        var futureFramePosition = transform.position + segmentSpeed * ScaledDeltaTime;
        RecordFutureFramePosition(currentFrame + 1, futureFramePosition);
        currentFrame++;
    }

    public void StartRecording(Vector3 centerPosition, float radius)
    {
        Log("Start Recording");
        khonsuSphereCenter = centerPosition;
        khonsuSphereRadius = radius;
        isRecording = true;
        currentFrame = 0;
        currentRecordingTime = 0;
        Array.ForEach(renderers, r => r.material = resume);
        RecordInitialFrame();
        state = RecorderState.Record;
    }

    public void StopRecording()
    {
        Log("Stop Recording");
        Resume();
        state = RecorderState.None;
        isRecording = false;
        Array.ForEach(renderers, r => r.material = _default);
        positions.ClearAll();
        rotations.ClearAll();
        wayPoints.ClearAll();
        PitchTimeScale(1f);
    }

    private void Log(string v)
    {
        // Debug.Log($"{gameObject.name}: {v}");
    }

    public bool IsMovingForward()
    {
        return state == RecorderState.Record && currentFrame > 1;
    }

    public bool IsInSphere()
    {
        return MathUtils.IsPositionInsideOfSphere(transform.position, khonsuSphereCenter, khonsuSphereRadius);
    }
}
