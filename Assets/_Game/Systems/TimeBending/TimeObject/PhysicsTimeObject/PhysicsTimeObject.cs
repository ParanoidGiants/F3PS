using UnityEngine;
using static UnityEngine.ParticleSystem;

public class PhysicsTimeObject : TimeObject
{
    protected const double TOLERANCE = 0.001f;
    protected float _defaultMass;
    protected Rigidbody _rigidbody;

    [Space(10)]
    [Header("Physics Settings")]
    public bool useGravity = true;
    public float gravityScale = 1f;
    public bool isTimeFrozen = false;

    private void Awake()
    {
        InitReferences();
    }

    override protected void InitReferences()
    {
        base.InitReferences();
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.useGravity = false;
        _defaultMass = _rigidbody.mass;
    }

    void FixedUpdate()
    {
        if (!useGravity || isTimeFrozen) return;
        var force = Physics.gravity * (currentTimeScale * currentTimeScale * gravityScale);
        _rigidbody.AddForce(
            force,
            ForceMode.Acceleration
        );
    }

    override
    public void PitchTimeScale(float newTimeScale)
    {
        if (_rigidbody == null) return;

        if (newTimeScale != 1f)
        {
            newTimeScale *= additionalTimeScale;
        }
        float relation = currentTimeScale == 0f ? 1f : newTimeScale / currentTimeScale;
        base.PitchTimeScale(newTimeScale);
        if (newTimeScale > TOLERANCE)
        {
            _rigidbody.constraints = RigidbodyConstraints.None;
            _rigidbody.mass = _defaultMass / (newTimeScale*newTimeScale);
            _rigidbody.velocity *= relation;
            _rigidbody.angularVelocity *= relation;
        }
        else
        {
            _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }
    }
}
