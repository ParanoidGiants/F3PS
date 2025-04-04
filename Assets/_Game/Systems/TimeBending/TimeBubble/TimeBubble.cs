using System.Collections.Generic;
using UnityEngine;

public class TimeBubble : MonoBehaviour
{
    public List<PhysicsRecorder> physicsRecorder = new List<PhysicsRecorder>();
    public List<PlatformRecorder> transformRecorder = new List<PlatformRecorder>();
    public float timeScale = 1f;
    public float targetSize = 10f;
    void OnTriggerEnter(Collider other)
    {
        var physicsRecorder = other.GetComponent<PhysicsRecorder>();
        if (physicsRecorder != null)
        {
            if (physicsRecorder.isRecording) return;

            physicsRecorder.StartRecording(transform.position, targetSize * 0.5f, timeScale);
            this.physicsRecorder.Add(physicsRecorder);
            return;
        }

        var transformRecorder = other.GetComponent<PlatformRecorder>();
        if (transformRecorder != null)
        {
            if (transformRecorder.isRecording) return;

            transformRecorder.StartRecording(transform.position, transform.localScale.x * 0.5f);
            transformRecorder.PitchTimeScale(timeScale);
            this.transformRecorder.Add(transformRecorder);
            return;
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        var physicsRecorder = other.GetComponent<PhysicsRecorder>();
        if (physicsRecorder != null)
        {
            if (physicsRecorder.IsMovingForward())
            {
                physicsRecorder.ChangeDirectionToPlayback();
            }
            return;
        }

        var transformRecorder = other.GetComponent<PlatformRecorder>();
        if (transformRecorder != null)
        {
            if (transformRecorder.IsMovingForward())
            {
                transformRecorder.ChangeDirectionToPlayback();
            }
            return;
        }

    }

    private void FixedUpdate()
    {
        foreach (var  recorder in physicsRecorder)
        {
            recorder.OnFixedUpdate();
        }
    }

    private void Update()
    {
        foreach (var recorder in physicsRecorder)
        {
            recorder.OnUpdate();
        }
        foreach (var recorder in transformRecorder)
        {
            recorder.OnUpdate();
        }
    }

    public void Clear()
    {
        foreach (var recorder in physicsRecorder)
        {
            recorder.StopRecording();
        }
        physicsRecorder.Clear();

        foreach (var recorder in transformRecorder)
        {
            recorder.StopRecording();
        }
        transformRecorder.Clear();
    }

    public void PitchTimeScale(float bubbleTimeScaleDirection)
    {
        timeScale += bubbleTimeScaleDirection;
        timeScale = Mathf.Clamp(timeScale, 0f, 1f);
        foreach (var recorder in physicsRecorder)
        {
            recorder.PitchTimeScale(timeScale);
        }
        foreach (var recorder in transformRecorder)
        {
            recorder.PitchTimeScale(timeScale);
        }
    }
}
