public class PhysicsTimeObject : TimeObject
{
    protected const double TOLERANCE = 0.001f;
    protected float _defaultMass;

    protected RigidbodyHub _rigidbodyHub;

    private void Awake()
    {
        InitReferences();
    }

    override protected void InitReferences()
    {
        base.InitReferences();
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
        _rigidbodyHub.SetTimeScale(newTimeScale);
    }
}
