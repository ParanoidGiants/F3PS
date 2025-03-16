using UnityEngine;

public class PhysicsTimeObject : TimeObject
{
    protected const double TOLERANCE = 0.0001f;
    protected float _defaultMass;
    private Rigidbody _rb;

    [Space(10)]
    [Header("Physics Settings")]
    public float gravityScale = 1f;

    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _defaultMass = _rb.mass;
    }

    void FixedUpdate()
    {
        if (_rb.isKinematic) return;
        
        var force = Physics.gravity * (currentTimeScale * currentTimeScale * gravityScale);
        _rb.AddForce(
            force,
            ForceMode.Acceleration
        );
    }

    override
    public void PitchTimeScale(float newTimeScale)
    {
        if (_rb == null) return;

        if (newTimeScale != 1f)
        {
            newTimeScale *= additionalTimeScale;
        }
        float relation = newTimeScale / currentTimeScale;
        base.PitchTimeScale(newTimeScale);
        
        if (newTimeScale > TOLERANCE)
        {
            _rb.isKinematic = false;
            _rb.mass = _defaultMass / (newTimeScale*newTimeScale);
            _rb.velocity *= relation;
            _rb.angularVelocity *= relation;
        }
        else
        {
            _rb.isKinematic = true;
            _rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }
}
