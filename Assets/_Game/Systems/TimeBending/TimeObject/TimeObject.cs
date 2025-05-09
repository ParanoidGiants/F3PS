using UnityEngine;

public class TimeObject : MonoBehaviour
{
    [Header("Watchers")]
    public OutlineTimeObject outline;
    public int amountOfTimeZones = 0;
    public float currentTimeScale = 1;
    public float additionalTimeScale = 1;
    public float ScaledDeltaTime => currentTimeScale * Time.deltaTime;

    private void Awake()
    {
        InitReferences();
    }

    private void Start()
    {
        PitchTimeScale(currentTimeScale);
    }

    protected virtual void InitReferences()
    {
        outline = GetComponentInChildren<OutlineTimeObject>(true);
    }

    public virtual void PitchTimeScale(float newTimeScale)
    {
        if (currentTimeScale == newTimeScale)
        {
            return;
        }

        outline.Pitch(newTimeScale);

        currentTimeScale = newTimeScale;
    }

    public virtual void Deactivate()
    {
        outline.Deactivate();
        PitchTimeScale(1f);
    }

    public virtual void Activate(float initialTimeScale)
    {
        outline.Activate();
        PitchTimeScale(initialTimeScale);
    }

    protected virtual void OnDisable()
    {
        PitchTimeScale(1f);
        amountOfTimeZones = 0;
    }
}
