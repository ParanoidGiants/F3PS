using System;
using UnityEngine;

public enum PhysicsRecorderState
{
    None,
    Record,
    Playback,
    Floating
}

public static class EqualityUtils
{
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
    private Renderer _renderer;

    public PropertyRecorder<Vector3> positions = new();
    public PropertyRecorder<Quaternion> rotations = new();
    public PropertyRecorder<Vector3> velocities = new();
    public PropertyRecorder<Vector3> angularVelocities = new();

    [Header("Playback")]
    public int playbackSpeed = 0;

    [Space(20)]
    [Header("Debug")]
    public Material _default;
    public Material resume;
    public Material rewind;
    public Material floating;

    [Space(20)]
    [Header("Watchers")]
    public int currentFrame = 0;
    public float floatingTolerance = 0.1f;
    public Vector3 timeBubbleCenter;
    public float timeBubbleRadius;
    public PhysicsRecorderState state;
    public bool isRecording = false;
    public bool isFloatingTheFirstTime = false;
    public Vector3 _floatingVelocity;

    public bool IsFloating => state == PhysicsRecorderState.Floating;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();
        state = PhysicsRecorderState.None;
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
        Debug.Log("Record");
        positions.RecordIfChanged(currentFrame, transform.position, EqualityUtils.Vector3Equals);
        rotations.RecordIfChanged(currentFrame, transform.rotation, EqualityUtils.QuaternionEquals);
        velocities.RecordIfChanged(currentFrame, _rigidbody.velocity, EqualityUtils.Vector3Equals);
        angularVelocities.RecordIfChanged(currentFrame, _rigidbody.angularVelocity, EqualityUtils.Vector3Equals);

        currentFrame++;
    }


    private void Playback()
    {
        Debug.Log("Playback");
        if (currentFrame <= 0)
        {
            ChangeDirectionToRecord();
            return;
        }
        transform.position = positions.GetValueAtFrame(currentFrame);
        transform.rotation = rotations.GetValueAtFrame(currentFrame);
        currentFrame--;
    }

    private void Resume()
    {
        Debug.Log("Resume");
        _renderer.material = resume;

        _rigidbody.useGravity = true;
        _rigidbody.isKinematic = false;
        _rigidbody.constraints = RigidbodyConstraints.None;

        _rigidbody.velocity = velocities.GetValueAtFrame(currentFrame);
        _rigidbody.angularVelocity = angularVelocities.GetValueAtFrame(currentFrame);
    }

    public void OnFixedUpdate()
    {
        Debug.Log("--------------------------");
        Debug.Log($"Frame: {currentFrame}");
        switch (state)
        {
            case PhysicsRecorderState.None:
                break;

            case PhysicsRecorderState.Record:
                Debug.Log("Handle Recording");
                Record();
                break;

            case PhysicsRecorderState.Playback:
                Debug.Log("Handle Playback");
                Playback();
                break;

            default:
                break;
        }
    }

    private void RestoreInitialFrame()
    {
        Debug.Log("Restore Initial Frame");
        transform.position = positions.GetValueAtFrame(0);
        transform.rotation = rotations.GetValueAtFrame(0);
        _rigidbody.velocity = velocities.GetValueAtFrame(0);
        _rigidbody.angularVelocity = angularVelocities.GetValueAtFrame(0);
        currentFrame = 0;
    }

    public void ChangeDirectionToRecord()
    {
        Debug.Log("Change Direction to Record");
        RestoreInitialFrame();
        state = PhysicsRecorderState.Record;
        ClearAllExceptFirstFrame();
        Resume();
        currentFrame++;
    }
    public void ChangeDirectionToPlayback()
    {
        Debug.Log("Change Direction to Playback");
        _renderer.material = rewind;
        _rigidbody.isKinematic = true;
        _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        state = PhysicsRecorderState.Playback;
        currentFrame--;
    }

    public void StartRecording(Vector3 centerPosition, float radius)
    {
        Debug.Log("Start Recording");
        timeBubbleCenter = centerPosition;
        timeBubbleRadius = radius;
        isRecording = true;
        _renderer.material = resume;
        state = PhysicsRecorderState.Record;
    }

    public void StopRecording()
    {
        Debug.Log("Stop Recording");
        Resume();
        isFloatingTheFirstTime = false;
        currentFrame = 0;
        state = PhysicsRecorderState.None;
        isRecording = false;
        _renderer.material = _default;
        ClearAll();
        
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

}
