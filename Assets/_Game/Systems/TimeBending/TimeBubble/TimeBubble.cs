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
            timeObjects.Add(timeObject);
            timeObject.PitchTimeScale(timeScale);
            return;
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        var timeObject = other.GetComponent<TimeObject>();
        if (timeObject != null)
        {
            timeObjects.Remove(timeObject);
            timeObject.PitchTimeScale(1f);
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
        foreach (var timeObject in timeObjects)
        {
            timeObject.PitchTimeScale(timeScale);
        }
    }
}
