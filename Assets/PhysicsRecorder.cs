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

    [Header("Materials")]
    public RewindOutline outline;


    [Space(10)]
    [Header("Physics Settings")]
    public int playbackSpeed = 0;
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
        outline.Init(GetComponent<MeshFilter>().mesh);
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
        Log("Freeze");
        _rigidbodyHub.FreezeAll();
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
        velocities.RecordIfChanged(currentFrame, _rigidbodyHub.GetUnbiasedVelocity(), MathUtils.Vector3Equals);
        angularVelocities.RecordIfChanged(currentFrame, _rigidbodyHub.GetUnbiasedAngularVelocity(), MathUtils.Vector3Equals);
        framesRecorded = currentFrame;
    }

    private void RecordInitialFrame()
    {
        currentFrame = 0;
        Log($"Record Initial Frame {currentFrame}: {transform.position}");
        positions.RecordIfChanged(currentFrame, transform.position, MathUtils.Vector3Equals);
        rotations.RecordIfChanged(currentFrame, transform.rotation, MathUtils.QuaternionEquals);
        velocities.RecordIfChanged(currentFrame, _rigidbodyHub.GetUnbiasedVelocity(), MathUtils.Vector3Equals);
        angularVelocities.RecordIfChanged(currentFrame, _rigidbodyHub.GetUnbiasedAngularVelocity(), MathUtils.Vector3Equals);
    }

    private void RecordFutureFramePosition(int frame, Vector3 position)
    {
        Log($"Record Specific Frame {frame}: {position}");
        positions.RecordIfChanged(frame, position, MathUtils.Vector3Equals);
        rotations.RecordIfChanged(frame, transform.rotation, MathUtils.QuaternionEquals);
        velocities.RecordIfChanged(frame, _rigidbodyHub.GetUnbiasedVelocity(), MathUtils.Vector3Equals);
        angularVelocities.RecordIfChanged(frame, _rigidbodyHub.GetUnbiasedAngularVelocity(), MathUtils.Vector3Equals);
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
        transform.position = positions.GetValueAtFrame(currentFrame);
        transform.rotation = rotations.GetValueAtFrame(currentFrame);

        outline.Record();
        _rigidbodyHub.FreeFromConstraints();
        _rigidbodyHub.SetUnbiasedVelocity(velocities.GetValueAtFrame(currentFrame));
        _rigidbodyHub.SetUnbiasedAngularVelocity(angularVelocities.GetValueAtFrame(currentFrame));

        state = RecorderState.Record;
        ClearAllAfterCurrentFrame(currentFrame);
    }

    public void SetupForPlayback()
    {
        outline.Rewind();
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

    public void StartRecording()
    {
        Log($"Start Recording {transform.position}");
        currentFrame = 0;
        currentRecordingTime = 0f;
        isRecording = true;
        outline.Record();

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
    }

    private void Log(string v)
    {
        // Debug.Log($"{gameObject.name}: {v}");
    }

    public bool IsMovingForward()
    {
        return state == RecorderState.Record && currentFrame > 1;    
    }

    private void ClearAllAfterCurrentFrame(int currentFrame)
    {
        positions.ClearAllAfterCurrentFrame(currentFrame);
        rotations.ClearAllAfterCurrentFrame(currentFrame);
        velocities.ClearAllAfterCurrentFrame(currentFrame);
        angularVelocities.ClearAllAfterCurrentFrame(currentFrame);
    }

    internal float GetPlaybackPercentage()
    {
        return currentFrame / (float)framesRecorded;
    }
}
