using System.Collections.Generic;
using UnityEngine;

public class TimeBubble : MonoBehaviour
{
    public List<TimeObject> timeObjects = new List<TimeObject>();
    public float timeScale = 1f;
    public float targetSize = 10f;
    void OnTriggerEnter(Collider other)
    {
        var timeObject = other.GetComponent<TimeObject>();
        if (timeObject != null)
        {
            this.timeObjects.Add(timeObject);
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
                physicsRecorder.ChangeToPlayback();
            }
            return;
        }
    }

    public void Clear()
    {
        foreach (var timeObject in timeObjects)
        {
            timeObject.PitchTimeScale(1f);
        }
        timeObjects.Clear();
    }

    public void PitchTimeScale(float bubbleTimeScaleDirection)
    {
        timeScale += bubbleTimeScaleDirection;
        timeScale = Mathf.Clamp(timeScale, 0f, 1f);
        foreach (var recorder in timeObjects)
        {
            recorder.PitchTimeScale(timeScale);
        }
    }
}
