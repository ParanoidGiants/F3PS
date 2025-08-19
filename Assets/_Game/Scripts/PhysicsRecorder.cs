using F3PS;
using System;
using UnityEngine;

public class PhysicsRecorder : MonoBehaviour
{
    private AnubisScrollSkillData AnubisScrollSkillData => GameManager.Instance.GameData.PlayerData.AnubisScrollSkillData;
    private PlayerEventController PlayerEventController => GameManager.Instance.GameData.PlayerEventController;

    private RigidbodyHub _rigidbodyHub;
    private const float THRESHOLD = 0.001f;

    public PropertyRecorder<Vector3> positions = new();
    public PropertyRecorder<Quaternion> rotations = new();
    public PropertyRecorder<Vector3> velocities = new();
    public PropertyRecorder<Vector3> angularVelocities = new();

    [Header("References")]
    public AnubisScrollOutline outline;
    public LineRenderer anubisScrollRecordingLine;
    public Mesh mesh;

    [Header("Recording Settings")]
    public int linePointFrequency = 5;

    public float ScaledDeltaTime => Time.deltaTime * _rigidbodyHub.currentTimeScale;
    public float ScaledFixedDeltaTime => Time.fixedDeltaTime * _rigidbodyHub.currentTimeScale;

    protected virtual void Awake()
    {
        outline.Init(mesh);
        _rigidbodyHub = GetComponent<RigidbodyHub>();
    }

    private void OnDisable()
    {
        if (AnubisScrollSkillData.State != AnubisScrollState.Record)
        {
            return;
        }

        StopRecording();
        PlayerEventController.SetAnubisScrollState(AnubisScrollState.None);
    }

    public void StartRecording()
    {
        outline.Record();
        positions.RecordIfChanged(0, _rigidbodyHub.Position, MathUtils.Vector3Equals);
        rotations.RecordIfChanged(0, _rigidbodyHub.Rotation, MathUtils.QuaternionEquals);
        velocities.RecordIfChanged(0, _rigidbodyHub.GetCurrentUnbiasedVelocity(), MathUtils.Vector3Equals);
        angularVelocities.RecordIfChanged(0, _rigidbodyHub.GetCurrentUnbiasedAngularVelocity(), MathUtils.Vector3Equals);
        SetRecordingLinePoint(0);
    }
    public void StopRecording()
    {
        _rigidbodyHub.UnsetKinematic();
        outline.gameObject.SetActive(false);
        positions.ClearAll();
        rotations.ClearAll();
        velocities.ClearAll();
        angularVelocities.ClearAll();
        anubisScrollRecordingLine.positionCount = 0;
    }

    public void Record()
    {
        var currentFrame = AnubisScrollSkillData.CurrentFrame;
        if (MathUtils.Vector3Equals(_rigidbodyHub.Position, positions.GetValueAtFrame(currentFrame), THRESHOLD)
                && MathUtils.QuaternionEquals(_rigidbodyHub.Rotation, rotations.GetValueAtFrame(currentFrame), THRESHOLD)
            )
        {
            return;
        }

        var currentRecordingTime = AnubisScrollSkillData.CurrentRecordingTime;
        var frameDuration = AnubisScrollSkillData.FrameDuration;
        currentRecordingTime += ScaledFixedDeltaTime;
        var nextFrame = (int)Math.Ceiling(currentRecordingTime / frameDuration);
        PlayerEventController.SetAnubisScrollCurrentRecordingTime(currentRecordingTime);

        positions.RecordIfChanged(nextFrame, _rigidbodyHub.Position, MathUtils.Vector3Equals);
        rotations.RecordIfChanged(nextFrame, _rigidbodyHub.Rotation, MathUtils.QuaternionEquals);
        velocities.RecordIfChanged(nextFrame, _rigidbodyHub.GetCurrentUnbiasedVelocity(), MathUtils.Vector3Equals);
        angularVelocities.RecordIfChanged(nextFrame, _rigidbodyHub.GetCurrentUnbiasedAngularVelocity(), MathUtils.Vector3Equals);
        SetRecordingLinePoint(nextFrame);

        if (currentFrame >= nextFrame)
        {
            return;
        }

        PlayerEventController.SetAnubisScrollCurrentFrame(nextFrame + 1);
        PlayerEventController.SetAnubisScrollTotalFrames(nextFrame + 1);
    }

    private void SetRecordingLinePoint(int frame)
    {
        int linePointIndex = (frame + linePointFrequency - 1) / linePointFrequency;
        anubisScrollRecordingLine.positionCount = linePointIndex + 1;
        anubisScrollRecordingLine.SetPosition(linePointIndex, _rigidbodyHub.Position);
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

    public void Playback(float forwardBackward)
    {
        if (forwardBackward == 0)
        {
            PlayerEventController.SetAnubisScrollState(AnubisScrollState.Paused);
            outline.Pause();
            return;
        }
        
        if (forwardBackward < 0)
        {
            PlayerEventController.SetAnubisScrollState(AnubisScrollState.Rewind);
            outline.Rewind();
        }
        else if (forwardBackward > 0)
        {
            PlayerEventController.SetAnubisScrollState(AnubisScrollState.Playback);
            outline.Resume();
        }

        var currentRecordingTime = AnubisScrollSkillData.CurrentRecordingTime;
        var frameDuration = AnubisScrollSkillData.FrameDuration;
        var maximumRecordingTime = AnubisScrollSkillData.TotalFrames * frameDuration;

        currentRecordingTime += ScaledDeltaTime * forwardBackward;
        if (currentRecordingTime <= 0)
        {
            PlayerEventController.SetAnubisScrollCurrentFrame(0);
            PlayerEventController.SetAnubisScrollCurrentRecordingTime(0);
            return;
        }
        else if (currentRecordingTime >= maximumRecordingTime)
        {
            PlayerEventController.SetAnubisScrollCurrentRecordingTime(maximumRecordingTime);
            PlayerEventController.SetAnubisScrollCurrentFrame(AnubisScrollSkillData.TotalFrames);
            return;
        }
        else
        {
            int currentFrame = (int)(currentRecordingTime / frameDuration);
            PlayerEventController.SetAnubisScrollCurrentRecordingTime(currentRecordingTime);
            PlayerEventController.SetAnubisScrollCurrentFrame(currentFrame);

            _rigidbodyHub.MovePosition(GetPositionAtCurrentTime());
            _rigidbodyHub.MoveRotation(GetRotationAtCurrentTime());

        }
    }

    public void SetupForRecording()
    {
        outline.Record();
        _rigidbodyHub.MovePosition(GetPositionAtCurrentTime());
        _rigidbodyHub.MoveRotation(GetRotationAtCurrentTime());
        _rigidbodyHub.SetupForRecording(GetVelocityAtCurrentTime(), GetAngularVelocityAtCurrentTime());

        var currentFrame = AnubisScrollSkillData.CurrentFrame;
        positions.ClearAllAfterCurrentFrame(currentFrame);
        rotations.ClearAllAfterCurrentFrame(currentFrame);
        velocities.ClearAllAfterCurrentFrame(currentFrame);
        angularVelocities.ClearAllAfterCurrentFrame(currentFrame);
    }

    private Quaternion GetRotationAtCurrentTime()
    {
        var currentRecordingTime = AnubisScrollSkillData.CurrentRecordingTime;
        var frameDuration = AnubisScrollSkillData.FrameDuration;
        int currentFrame = (int)(currentRecordingTime / frameDuration);
        int nextFrame = currentFrame + 1;
        if (nextFrame >= AnubisScrollSkillData.TotalFrames)
        {
            return rotations.GetValueAtFrame(currentFrame);
        }
        float lerpFactor = (currentRecordingTime % frameDuration) / frameDuration;
        var currentRot = rotations.GetValueAtFrame(currentFrame);
        var nextRot = rotations.GetValueAtFrame(nextFrame);
        return Quaternion.Slerp(currentRot, nextRot, lerpFactor);
    }

    private Vector3 GetPositionAtCurrentTime()
    {
        var currentRecordingTime = AnubisScrollSkillData.CurrentRecordingTime;
        var frameDuration = AnubisScrollSkillData.FrameDuration;
        int currentFrame = (int)(currentRecordingTime / frameDuration);
        int nextFrame = currentFrame + 1;
        if (nextFrame >= AnubisScrollSkillData.TotalFrames)
        {
            return positions.GetValueAtFrame(currentFrame);
        }

        float lerpFactor = (currentRecordingTime % frameDuration) / frameDuration;
        Vector3 currentPos = positions.GetValueAtFrame(currentFrame);
        Vector3 nextPos = positions.GetValueAtFrame(nextFrame);
        return Vector3.Lerp(currentPos, nextPos, lerpFactor);
    }

    private Vector3 GetVelocityAtCurrentTime()
    {
        var currentRecordingTime = AnubisScrollSkillData.CurrentRecordingTime;
        var frameDuration = AnubisScrollSkillData.FrameDuration;
        int currentFrame = (int)(currentRecordingTime / frameDuration);
        int nextFrame = currentFrame + 1;
        if (nextFrame >= AnubisScrollSkillData.TotalFrames)
        {
            return velocities.GetValueAtFrame(currentFrame);
        }
        float lerpFactor = (currentRecordingTime % frameDuration) / frameDuration;
        Vector3 currentVel = velocities.GetValueAtFrame(currentFrame);
        Vector3 nextVel = velocities.GetValueAtFrame(nextFrame);
        return Vector3.Lerp(currentVel, nextVel, lerpFactor);
    }

    private Vector3 GetAngularVelocityAtCurrentTime()
    {
        var currentRecordingTime = AnubisScrollSkillData.CurrentRecordingTime;
        var frameDuration = AnubisScrollSkillData.FrameDuration;
        int currentFrame = (int)(currentRecordingTime / frameDuration);
        int nextFrame = currentFrame + 1;
        if (nextFrame >= AnubisScrollSkillData.TotalFrames)
        {
            return angularVelocities.GetValueAtFrame(currentFrame);
        }
        float lerpFactor = (currentRecordingTime % frameDuration) / frameDuration;
        Vector3 currentAngVel = angularVelocities.GetValueAtFrame(currentFrame);
        Vector3 nextAngVel = angularVelocities.GetValueAtFrame(nextFrame);
        return Vector3.Lerp(currentAngVel, nextAngVel, lerpFactor);
    }

    public void SetupForPlayback()
    {
        outline.Rewind();
        _rigidbodyHub.SetupForPlayback();
    }
}
