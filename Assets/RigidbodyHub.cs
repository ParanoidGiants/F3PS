using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class RigidbodyHub : MonoBehaviour
{
    private const double TIME_SCALE_TOLERANCE = 0.001f;

    public Rigidbody _rigidbody;
    public bool useGravity = true;
    public float defaultMass;
    public float currentGravityScale = 1.0f;
    public float currentTimeScale = 1.0f;
    public bool isTimeFrozen = false;
    public Vector3 unbiasedTimeFreezeAngularVelocity;
    public Vector3 unbiasedTimeFreezeVelocity;


    public Vector3 GetUnbiasedAngularVelocity()
    {
        if (isTimeFrozen)
        {
            return unbiasedTimeFreezeAngularVelocity;
        }
        return _rigidbody.angularVelocity / currentTimeScale;
    }
    public Vector3 GetUnbiasedVelocity()
    {
        if (isTimeFrozen)
        {
            return unbiasedTimeFreezeVelocity;
        }
        return _rigidbody.velocity / currentTimeScale;
    }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.useGravity = false;
        defaultMass = _rigidbody.mass;
    }

    void FixedUpdate()
    {
        if (!useGravity || isTimeFrozen) return;
        var force = Physics.gravity * (currentTimeScale * currentTimeScale * currentGravityScale);
        _rigidbody.AddForce(
            force,
            ForceMode.Acceleration
        );
    }

    public void SetTimeScale(float timeScale)
    {
        float relation = currentTimeScale == 0f ? 1f : timeScale / currentTimeScale;
        if (timeScale > TIME_SCALE_TOLERANCE)
        {
            if (isTimeFrozen)
            {
                isTimeFrozen = false;
                if (!isMovingByTelekinesis)
                {
                    _rigidbody.constraints = RigidbodyConstraints.None;
                    _rigidbody.velocity = unbiasedTimeFreezeVelocity * timeScale;
                    _rigidbody.angularVelocity = unbiasedTimeFreezeAngularVelocity * timeScale;
                }
            }
            else
            {
                if (!isMovingByTelekinesis)
                {
                    _rigidbody.velocity *= relation;
                    _rigidbody.angularVelocity *= relation;
                }
            }
            if (!isMovingByTelekinesis)
            {
                _rigidbody.mass = defaultMass / (timeScale * timeScale);
            }
        }
        else if (!isTimeFrozen)
        {
            isTimeFrozen = true;
            if (!isMovingByTelekinesis)
            {
                unbiasedTimeFreezeVelocity = _rigidbody.velocity / currentTimeScale;
                unbiasedTimeFreezeAngularVelocity = _rigidbody.angularVelocity / currentTimeScale;
                _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            }
        }
        currentTimeScale = timeScale;
    }

    public void FreezeAll()
    {
        _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
    }

    public void FreeFromConstraints()
    {
        _rigidbody.constraints = RigidbodyConstraints.None;
    }

    public void SetUnbiasedVelocity(Vector3 unbiasedVelocity)
    {
        if (isTimeFrozen)
        {
            unbiasedTimeFreezeVelocity = unbiasedVelocity;
        }
        else
        {
            _rigidbody.velocity = unbiasedVelocity / currentTimeScale;
        }
    }

    public void SetUnbiasedAngularVelocity(Vector3 unbiasedAngularVelocity)
    {
        if (isTimeFrozen)
        {
            unbiasedTimeFreezeAngularVelocity = unbiasedAngularVelocity;
        }
        else
        {
            _rigidbody.angularVelocity = unbiasedAngularVelocity / currentTimeScale;
        }
    }

    public bool isMovingByTelekinesis = false;
    internal void StartTelekinesisMoving()
    {
        isMovingByTelekinesis = true;
        useGravity = false;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.mass = defaultMass;
    }

    internal void StopTelekinesisMoving(float maximumThrowSpeed)
    {
        isMovingByTelekinesis = false;
        useGravity = true;
        var throwVelocity = Vector3.ClampMagnitude(_rigidbody.velocity, maximumThrowSpeed);
        if (isTimeFrozen)
        {
            _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            SetUnbiasedVelocity(throwVelocity);
        }
        else
        {
            _rigidbody.constraints = RigidbodyConstraints.None;
            _rigidbody.mass = defaultMass / (currentTimeScale * currentTimeScale);
            _rigidbody.velocity = throwVelocity * currentTimeScale;
            _rigidbody.angularVelocity = Vector3.zero;
        }
    }

    internal void SetTelekinesisVelocity(Vector3 velocity)
    {
        _rigidbody.velocity = velocity;
    }
}
