using System;
using UnityEngine;

public class TimeObject : MonoBehaviour
{
    public int amountOfTimeZones = 0;
    public float currentTimeScale = 1;

    void Start()
    {
        PitchTimeScale(currentTimeScale);
    }

    public virtual void PitchTimeScale(float newTimeScale)
    {
        currentTimeScale = newTimeScale;
    }

    private void OnDisable()
    {
        PitchTimeScale(1f);
        amountOfTimeZones = 0;
    }
}
