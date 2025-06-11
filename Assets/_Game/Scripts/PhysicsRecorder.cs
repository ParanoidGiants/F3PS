using System;
using UnityEngine;

public enum RecorderState
{
    None,
    Record,
    Playback
}

public class PhysicsRecorder : MonoBehaviour
{
    private RigidbodyHub _rigidbodyHub;

    public PropertyRecorder<Vector3> positions = new();
    public PropertyRecorder<Quaternion> rotations = new();
    public PropertyRecorder<Vector3> velocities = new();
    public PropertyRecorder<Vector3> angularVelocities = new();

    [Header("References")]
    public RewindOutline outline;
    public LineRenderer rewindLine;
    public MeshFilter meshFilter;


    [Space(10)]
    [Header("Physics Settings")]
    public int playbackSpeed = 0;
    public int amountOfTimeZones = 0;
    public float currentTimeScale = 1;
    public float additionalTimeScale = 1;
    public float ScaledDeltaTime => currentTimeScale * Time.deltaTime * _rigidbodyHub.currentTimeScale;
    public float ScaledFixedDeltaTime => currentTimeScale * Time.fixedDeltaTime * _rigidbodyHub.currentTimeScale;

    public float gravityScale = 1f;
    protected const double TOLERANCE = 0.001f;
    public float defaultMass;
    public float initialTimeScale = 1f;
    public float frameDuration = 0.02f;

    [Space(20)]
    [Header("Watchers")]
    public float currentRecordingTime = 0;
    public int currentFrame = 0;
    public int framesRecorded = 0;
    public RecorderState state;
    public bool isRecording = false;

    protected virtual void Awake()
    {
        state = RecorderState.None;
        outline.Init(meshFilter.mesh);
        _rigidbodyHub = GetComponent<RigidbodyHub>();
    }

    public void FixedUpdate()
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
    public void SelectAsCandidate()
    {
        outline.SetActive(true);
        outline.Pick();
    }

    public void Unpick()
    {
        outline.SetActive(false);
    }

    private void FreezeRigidbody()
    {
    }

    private void OnDisable()
    {
        if (isRecording)
        {
            StopRecording();
        }
    }

    private const float THRESHOLD = 0.001f;
    private void Record()
    {
        var nextFrame = (int) Math.Floor(currentRecordingTime / frameDuration);
        Log($"Next Frame: {nextFrame}");
        Log($"Current Recording Time: {currentRecordingTime}");
        Log($"Current Fixed Delta Time: {ScaledFixedDeltaTime}");
        Log($"Current Time Scale: {currentTimeScale}");
        if (MathUtils.Vector3Equals(transform.position, positions.GetValueAtFrame(currentFrame), THRESHOLD)
                && MathUtils.QuaternionEquals(transform.rotation, rotations.GetValueAtFrame(currentFrame), THRESHOLD)
            )
        {
            return;
        }
        currentFrame = nextFrame;
        int linePointIndex = currentFrame / 20;
        rewindLine.positionCount = linePointIndex+1;
        rewindLine.SetPosition(linePointIndex, transform.position);
        Log($"Record Frame {currentFrame}: {transform.position}");
        positions.RecordIfChanged(currentFrame, transform.position, MathUtils.Vector3Equals);
        rotations.RecordIfChanged(currentFrame, transform.rotation, MathUtils.QuaternionEquals);
        velocities.RecordIfChanged(currentFrame, _rigidbodyHub.GetCurrentUnbiasedVelocity(), MathUtils.Vector3Equals);
        angularVelocities.RecordIfChanged(currentFrame, _rigidbodyHub.GetCurrentUnbiasedAngularVelocity(), MathUtils.Vector3Equals);
        framesRecorded = currentFrame;
        currentRecordingTime += ScaledFixedDeltaTime;
    }

    private void RecordInitialFrame()
    {
        currentFrame = 0;
        rewindLine.positionCount = 1;
        rewindLine.SetPosition(0, transform.position);
        Log($"Record Initial Frame {currentFrame}: {transform.position}");
        positions.RecordIfChanged(currentFrame, transform.position, MathUtils.Vector3Equals);
        rotations.RecordIfChanged(currentFrame, transform.rotation, MathUtils.QuaternionEquals);
        velocities.RecordIfChanged(currentFrame, _rigidbodyHub.GetCurrentUnbiasedVelocity(), MathUtils.Vector3Equals);
        angularVelocities.RecordIfChanged(currentFrame, _rigidbodyHub.GetCurrentUnbiasedAngularVelocity(), MathUtils.Vector3Equals);
    }

    public void Playback(float forwardBackward)
    {
        if (state != RecorderState.Playback)
        {
            state = RecorderState.Playback;
            SetupForPlayback();
        }

        if (forwardBackward == 0)
        {
            outline.Pause();
            return;
        }
        
        if (forwardBackward < 0)
        {
            outline.Rewind();
        }
        else if (forwardBackward > 0)
        {
            outline.Resume();
        }

        currentRecordingTime += ScaledDeltaTime * forwardBackward;
        if (currentRecordingTime < 0)
        {
            currentRecordingTime = 0f;
            currentFrame = 0;
            return;
        }
        else if (currentRecordingTime > framesRecorded)
        {
            currentRecordingTime = 0f;
            currentFrame = framesRecorded;
            return;
        }

        currentFrame = (int)(currentRecordingTime / frameDuration);
        int nextFrame = currentFrame + 1;
        float lerpFactor = (currentRecordingTime % frameDuration) / frameDuration;

        Vector3 currentPos = positions.GetValueAtFrame(currentFrame);
        Vector3 nextPos = positions.GetValueAtFrame(nextFrame);
        transform.position = Vector3.Lerp(currentPos, nextPos, lerpFactor);

        Quaternion currentRot = rotations.GetValueAtFrame(currentFrame);
        Quaternion nextRot = rotations.GetValueAtFrame(nextFrame);
        transform.rotation = Quaternion.Lerp(currentRot, nextRot, lerpFactor);
    }

    public void SetupForRecording()
    {
        Log($"Setup For Recording {currentFrame}");
        transform.position = positions.GetValueAtFrame(currentFrame);
        transform.rotation = rotations.GetValueAtFrame(currentFrame);

        outline.Record();
        _rigidbodyHub.SetupForRecording(velocities.GetValueAtFrame(currentFrame), angularVelocities.GetValueAtFrame(currentFrame));

        positions.ClearAllAfterCurrentFrame(currentFrame);
        rotations.ClearAllAfterCurrentFrame(currentFrame);
        velocities.ClearAllAfterCurrentFrame(currentFrame);
        angularVelocities.ClearAllAfterCurrentFrame(currentFrame);

        state = RecorderState.Record;
    }

    public void SetupForPlayback()
    {
        Log($"Setup For Playback {currentFrame}");
        outline.Rewind();
        _rigidbodyHub.SetupForPlayback();
        state = RecorderState.Playback;
    }

    public void StartRecording()
    {
        Log($"Start Recording {transform.position}");
        currentFrame = 0;
        currentRecordingTime = 0f;
        isRecording = true;
        outline.Record();
        rewindLine.positionCount = 0;

        RecordInitialFrame();
        state = RecorderState.Record;
    }

    public void StopRecording()
    {
        _rigidbodyHub.FreeFromConstraints();
        currentFrame = 0;
        state = RecorderState.None;
        isRecording = false;
        outline.gameObject.SetActive(false);
        positions.ClearAll();
        rotations.ClearAll();
        velocities.ClearAll();
        angularVelocities.ClearAll();
        rewindLine.positionCount = 0;
    }

    private void Log(string v)
    {
        // Debug.Log($"{gameObject.name}: {v}");
    }

    internal float GetPlaybackPercentage()
    {
        if (framesRecorded == 0)
        {
            return 0f;
        }
        return currentFrame / (float)framesRecorded;
    }
}
