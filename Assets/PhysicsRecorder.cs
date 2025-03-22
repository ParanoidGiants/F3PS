using System;
using UnityEngine;

public enum PhysicsRecorderState
{
    None,
    Record,
    Rewind,
    Playback,
    Floating
}

public struct PhysicsFrame
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
    public Vector3 angularVelocity;
}

public class PhysicsRecorder : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private Renderer _renderer;
    private PhysicsFrame[] frames = new PhysicsFrame[1000000];

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
    public bool isInTimeBubble = false;
    public bool isFloatingTheFirstTime = false;
    public Vector3 _floatingVelocity;

    public bool IsFloating => state == PhysicsRecorderState.Floating;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();
        state = PhysicsRecorderState.None;
    }
    private void Record()
    {
        var frame = new PhysicsFrame() {
            position = transform.position,
            rotation = transform.rotation,
            velocity = _rigidbody.velocity,
            angularVelocity = _rigidbody.angularVelocity
        };

        frames[currentFrame++] = frame;
    }


    private void RecordInitialFrame()
    {
        var frame = new PhysicsFrame()
        {
            position = transform.position,
            rotation = transform.rotation,
            velocity = _rigidbody.velocity,
            angularVelocity = _rigidbody.angularVelocity
        };

        frames[0] = frame;
    }


    private void Playback()
    {
        if (currentFrame <= 0)
        {
            currentFrame = 1;
            ChangeDirection();
            return;
        }
        transform.position = frames[currentFrame].position;
        transform.rotation = frames[currentFrame].rotation;
        currentFrame--;
    }

    private void Resume()
    {
        _renderer.material = resume;

        _rigidbody.useGravity = true;
        _rigidbody.isKinematic = false;
        _rigidbody.constraints = RigidbodyConstraints.None;

        _rigidbody.velocity = frames[currentFrame].velocity;
        _rigidbody.angularVelocity = frames[currentFrame].angularVelocity;
    }

    public void OnFixedUpdate()
    {
        switch (state)
        {
            case PhysicsRecorderState.None:
                break;

            case PhysicsRecorderState.Floating:
                _rigidbody.AddForce(_floatingVelocity, ForceMode.Acceleration);
                _rigidbody.velocity = Vector3.Project(_rigidbody.velocity, _floatingVelocity);
                var distanceToCenter = Vector3.Distance(timeBubbleCenter, transform.position);
                if (distanceToCenter < timeBubbleRadius)
                {
                    if (isFloatingTheFirstTime)
                    {
                        isFloatingTheFirstTime = false;
                        RecordInitialFrame();
                    }
                    RestoreInitialFrame();
                    Resume();
                    state = PhysicsRecorderState.Record;
                }
                break;

            case PhysicsRecorderState.Rewind:
                _renderer.material = rewind;
                _rigidbody.isKinematic = true;
                _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
                state = PhysicsRecorderState.Playback;
                currentFrame--;
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

    private void RestoreInitialFrame()
    {
        var frame = frames[0];
        transform.position = frame.position;
        transform.rotation = frame.rotation;
        _rigidbody.velocity = frame.velocity;
        _rigidbody.angularVelocity = frame.angularVelocity;
    }

    public void ChangeDirection()
    {
        if (state is PhysicsRecorderState.Playback)
        {
            _renderer.material = floating;
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = false;
            state = PhysicsRecorderState.Floating;
        }
        else if (state is PhysicsRecorderState.Record)
        {
            state = PhysicsRecorderState.Rewind;
        }
    }

    public void StartRecording(Vector3 centerPosition, float radius)
    {
        timeBubbleCenter = centerPosition;
        timeBubbleRadius = radius;
        isInTimeBubble = true;

        isFloatingTheFirstTime = true;

        _renderer.material = floating;
        _floatingVelocity = _rigidbody.velocity;
        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = false;
        state = PhysicsRecorderState.Floating;
    }

    public void StopRecording()
    {
        Resume();
        isFloatingTheFirstTime = false;
        state = PhysicsRecorderState.None;
        isInTimeBubble = false;
        _renderer.material = _default;
    }
}
