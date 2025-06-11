public class PhysicsTimeObject : TimeObject
{
    protected const double TOLERANCE = 0.001f;
    protected float _defaultMass;
    protected RigidbodyHub _rigidbodyHub;

    public OutlineTimeObject outline;

    private void Awake()
    {
        InitReferences();
    }

    override protected void InitReferences()
    {
        base.InitReferences();
        outline.Init();
        _rigidbodyHub = GetComponent<RigidbodyHub>();
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
        _rigidbodyHub.SetTimeScale(newTimeScale);
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
