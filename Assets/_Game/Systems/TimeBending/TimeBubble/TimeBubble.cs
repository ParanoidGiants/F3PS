using System.Collections.Generic;
using UnityEngine;

public class TimeBubble : MonoBehaviour
{
    public List<PhysicsRecorder> recorders = new List<PhysicsRecorder>();
    public float timeScale = 1f;
    void OnTriggerEnter(Collider other)
    {
        var o = other.GetComponent<PhysicsRecorder>();
        if (o == null || o.isRecording) return;

        o.StartRecording(transform.position, transform.localScale.x * 0.5f);
        o.PitchTimeScale(timeScale);
        recorders.Add(o);
    }
    
    void OnTriggerExit(Collider other)
    {
        var o = other.GetComponent<PhysicsRecorder>();
        if (o == null)
        {
            return;
        }
        
        if (!o.IsInSphere() && o.IsMovingForward())
        {
            o.ChangeDirectionToPlayback();
        }
    }

    private void FixedUpdate()
    {
        foreach (var  recorder in recorders)
        {
            recorder.OnFixedUpdate();
        }
    }

    private void OnDisable()
    {

        foreach (var recorder in recorders)
        {
            recorder.StopRecording();
        }
        recorders.Clear();
    }

    public void PitchTimeScale(float bubbleTimeScaleDirection)
    {
        timeScale += bubbleTimeScaleDirection;
        timeScale = Mathf.Clamp(timeScale, 0f, 1f);
        foreach (var recorder in recorders)
        {
            recorder.PitchTimeScale(timeScale);
        }
    }
}
