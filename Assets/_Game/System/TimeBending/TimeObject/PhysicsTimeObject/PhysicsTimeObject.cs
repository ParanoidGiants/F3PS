using UnityEngine;

public class PhysicsTimeObject : TimeObject
{
    protected const double TOLERANCE = 0.0001f;
    protected float _defaultMass;
    public Rigidbody rb;
    public float gravityScale = 1f;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        _defaultMass = rb.mass;
    }

    void FixedUpdate()
    {
        if (rb.isKinematic) return;
        
        var force = Physics.gravity * (currentTimeScale * currentTimeScale * gravityScale);
        rb.AddForce(
            force,
            ForceMode.Acceleration
        );
    }

    override
    public void PitchTimeScale(float newTimeScale)
    {
        if (rb == null) return;
        
        float relation = newTimeScale / currentTimeScale;
        base.PitchTimeScale(newTimeScale);
        
        if (newTimeScale > TOLERANCE)
        {
            rb.isKinematic = false;
            rb.mass = _defaultMass / (newTimeScale*newTimeScale);
            rb.velocity *= relation;
            rb.angularVelocity *= relation;
        }
        else
        {
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }
}
