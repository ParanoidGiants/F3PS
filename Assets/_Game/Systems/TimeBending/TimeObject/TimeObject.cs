using System;
using UnityEngine;

public class TimeObject : MonoBehaviour
{
    [Header("Watchers")]
    public int amountOfTimeZones = 0;
    public float currentTimeScale = 1;
    public float additionalTimeScale = 1;
    public float ScaledDeltaTime => currentTimeScale * Time.deltaTime;
    public float ScaledFixedDeltaTime => currentTimeScale * Time.fixedDeltaTime;
    public Action<float> OnTimeScaleChanged;

    private void Awake()
    {
        InitReferences();
    }

    private void Start()
    {
        PitchTimeScale(currentTimeScale);
    }

    protected virtual void InitReferences() {}

    public virtual void PitchTimeScale(float timeScale)
    {
        var newTimeScale = timeScale == 1f
            ? 1f
            : timeScale * additionalTimeScale;

        if (currentTimeScale == newTimeScale)
        {
            return;
        }
        currentTimeScale = newTimeScale;
        OnTimeScaleChanged?.Invoke(newTimeScale);
    }

    public virtual void Deactivate()
    {
        PitchTimeScale(1f);
    }

    public virtual void Activate(float initialTimeScale)
    {
        PitchTimeScale(initialTimeScale);
    }

    protected virtual void OnDisable()
    {
        PitchTimeScale(1f);
        amountOfTimeZones = 0;
    }
}
