using System;
using UnityEngine;

public class TimeObject : MonoBehaviour
{
    private const double TOLERANCE = 0.001f;
    public Rigidbody rb;
    private float _defaultMass;
    public int amountOfTimeZones = 0;
    public float currentTimeScale = 1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        _defaultMass = rb.mass;
    }

    void Start()
    {
        PitchTimeScale(currentTimeScale);
    }

    void FixedUpdate()
    {
        if (!rb.isKinematic)
        {
            var force = currentTimeScale * currentTimeScale * Physics.gravity;
            rb.AddForce(
                force,
                ForceMode.Acceleration
            );
        }
    }

    public void PitchTimeScale(float newTimeScale)
    {
        if (rb == null) return;
        
        float relation = newTimeScale / currentTimeScale;
        currentTimeScale = newTimeScale;
        
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

    private void OnDisable()
    {
        PitchTimeScale(1f);
        amountOfTimeZones = 0;
    }
}
