using UnityEngine;

public class PlatformTimeObject : TimeObject
{
    public OutlineTimeObject outline;

    private void Awake()
    {
        InitReferences();
    }

    override protected void InitReferences()
    {
        base.InitReferences();
        outline.Init();
    }

    override
    public void PitchTimeScale(float newTimeScale)
    {
        if (newTimeScale != 1f)
        {
            newTimeScale *= additionalTimeScale;
        }
        base.PitchTimeScale(newTimeScale);
        outline.Pitch(newTimeScale);
    }

    override
    public void Deactivate()
    {
        outline.Deactivate();
        base.Deactivate();
    }

    override
    public void Activate(float initialTimeScale)
    {
        outline.Activate();
        base.Activate(initialTimeScale);
    }
}
