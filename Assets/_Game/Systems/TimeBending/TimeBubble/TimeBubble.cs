using System.Collections.Generic;
using UnityEngine;

public class TimeBubble : MonoBehaviour
{
    public List<TimeObject> timeObjects = new List<TimeObject>();
    private Renderer _renderer;
    public float timeScale = 1f;
    public float targetSize = 10f;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    private void OnEnable()
    {
        _renderer.material.SetFloat("_BrackeysMoveSpeed", timeScale * 0.25f);
    }

    void OnTriggerEnter(Collider other)
    {
        var timeObject = other.GetComponent<TimeObject>();
        if (timeObject == null)
        {
            return;
        }
        timeObjects.Add(timeObject);
        timeObject.Activate(timeScale);
    }
    
    void OnTriggerExit(Collider other)
    {
        var timeObject = other.GetComponent<TimeObject>();
        if (timeObject == null)
        {
            return;
        }
        timeObjects.Remove(timeObject);
        timeObject.Deactivate();
    }

    public void Clear()
    {
        foreach (var timeObject in timeObjects)
        {
            timeObject.Deactivate();
        }
        timeObjects.Clear();
    }

    public void PitchTimeScale(float bubbleTimeScaleDirection)
    {
        timeScale += bubbleTimeScaleDirection;
        timeScale = Mathf.Clamp(timeScale, 0f, 1f);

        _renderer.material.SetFloat("_BrackeysMoveSpeed", timeScale*0.25f);
        foreach (var timeObject in timeObjects)
        {
            timeObject.PitchTimeScale(timeScale);
        }
    }
}
