using System;
using UnityEngine;

public class TimeObject : MonoBehaviour
{
    private const double TOLERANCE = 0.001f;
    private Rigidbody _rb;
    private float _defaultMass;
    public int amountOfTimeZones = 0;

    public float currentTimeScale;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _defaultMass = _rb.mass;
    }

    void Start()
    {
        PitchTimeScale(currentTimeScale);
    }
    
    public float normalModifier = 0f;
    public float scaledModifier = 0f;

    void FixedUpdate()
    {
        if (!_rb.isKinematic)
        {
            var force = currentTimeScale * currentTimeScale * Physics.gravity;
            _rb.AddForce(
                force,
                ForceMode.Acceleration
            );
        }
    }

    public void PitchTimeScale(float newTimeScale)
    {
        if (_rb == null) return;
        
        float relation = newTimeScale / currentTimeScale;
        currentTimeScale = newTimeScale;
        
        if (newTimeScale > TOLERANCE)
        {
            var scaledVelocity = _rb.velocity * relation;
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

    private void OnDisable()
    {
        PitchTimeScale(1f);
        amountOfTimeZones = 0;
    }
}
