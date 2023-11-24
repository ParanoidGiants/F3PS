using System.Collections.Generic;
using UnityEngine;

public class TimeBubble : MonoBehaviour
{
    private readonly List<TimeObject> _timeObjects = new List<TimeObject>();
    public float timeScale = 0.05f;
    void OnTriggerEnter(Collider other)
    {
        TimeObject o = other.GetComponent<TimeObject>();
        if (o != null)
        {
            if (o.amountOfTimeZones == 0)
            {
                o.PitchTimeScale(timeScale);
            }
            o.amountOfTimeZones++;
            _timeObjects.Add(o);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        TimeObject o = other.GetComponent<TimeObject>();
        if (o != null)
        {
            if (o.amountOfTimeZones == 1)
            {
                o.PitchTimeScale(1f);
            }
            o.amountOfTimeZones--;
            _timeObjects.Remove(o);
        }
    }

    private void OnDisable()
    {
        foreach (var timeObject in _timeObjects)
        {
            timeObject.amountOfTimeZones--;
            timeObject.PitchTimeScale(1f);
        }
        
        _timeObjects.Clear();
    }
}
