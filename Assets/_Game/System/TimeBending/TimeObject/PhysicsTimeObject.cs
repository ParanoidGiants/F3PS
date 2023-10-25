using UnityEngine;

public class PhysicsTimeObject : TimeObject
{
    private const double TOLERANCE = 0.001f;
    public Rigidbody rb;
    private float _defaultMass;
    public float gravityScale = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        _defaultMass = rb.mass;
    }

    void FixedUpdate()
    {
        if (rb.isKinematic) return;
        
        var force = currentTimeScale * currentTimeScale * Physics.gravity * gravityScale;
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
