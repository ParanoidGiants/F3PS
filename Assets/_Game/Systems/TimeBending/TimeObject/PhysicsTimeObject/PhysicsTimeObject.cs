using System;
using UnityEngine;

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
    public Vector3 timeFreezeVelocity = Vector3.zero;
    public Vector3 timeFreezeAngularVelocity = Vector3.zero;

    public Vector3 AngularVelocity => _rigidbody.angularVelocity / currentTimeScale;
    public Vector3 Velocity => _rigidbody.velocity / currentTimeScale;

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
            if (!isTimeFrozen)
            {
                _rigidbody.mass = _defaultMass / (newTimeScale*newTimeScale);
                _rigidbody.velocity *= relation;
                _rigidbody.angularVelocity *= relation;
            }
            else
            {
                _rigidbody.constraints = RigidbodyConstraints.None;
                _rigidbody.mass = _defaultMass / (newTimeScale*newTimeScale);
                _rigidbody.velocity = timeFreezeVelocity;
                _rigidbody.angularVelocity = timeFreezeAngularVelocity;
                isTimeFrozen = false;
            }
        }
        else if (!isTimeFrozen)
        {
            isTimeFrozen = true;
            timeFreezeVelocity = _rigidbody.velocity;
            timeFreezeAngularVelocity = _rigidbody.angularVelocity;
            _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    public bool isFrozenExternal = false;
    internal void FreeFromExternalConstraints()
    {
        if (!isTimeFrozen && isFrozenExternal)
        {
            isFrozenExternal = false;
            _rigidbody.constraints = RigidbodyConstraints.None;
        }
    }

    public void SetVelocity(Vector3 velocity, Vector3 angularVelocity)
    {
        float relation = 1f / currentTimeScale;
        _rigidbody.velocity = velocity * relation;
        _rigidbody.angularVelocity = angularVelocity * relation;
    }

    internal void FreezeExternally()
    {
        isFrozenExternal = true;
        _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
    }
}
