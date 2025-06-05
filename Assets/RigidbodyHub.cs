using System;
using UnityEngine;

public class RigidbodyHub : MonoBehaviour
{
    private const double TIME_SCALE_TOLERANCE = 0.001f;
    public Rigidbody _rigidbody;

    [Header("Time Bubble Settings")]
    public bool useGravity = true;
    public float defaultMass;
    public float currentGravityScale = 1.0f;
    public float currentTimeScale = 1.0f;
    public bool isTimeFrozen = false;

    [Space(10)]
    [Header("Telekinesis Settings")]
    public bool isMovingByTelekinesis = false;

    [Space(10)]
    [Header("Rewind Settings")]
    public bool isRewinding = false;

    [Space(10)]
    [Header("Watchers")]
    public int constraintsFreezeAllCommandCount = 0;
    public Vector3 unbiasedTimeFreezeAngularVelocity;
    public Vector3 unbiasedTimeFreezeVelocity;

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

    #region TIME SCALE
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
                    FreeFromConstraints();
                    _rigidbody.linearVelocity = unbiasedTimeFreezeVelocity * timeScale;
                    _rigidbody.angularVelocity = unbiasedTimeFreezeAngularVelocity * timeScale;
                }
            }
            else
            {
                if (!isMovingByTelekinesis)
                {
                    _rigidbody.linearVelocity *= relation;
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
                unbiasedTimeFreezeVelocity = _rigidbody.linearVelocity / currentTimeScale;
                unbiasedTimeFreezeAngularVelocity = _rigidbody.angularVelocity / currentTimeScale;
                FreezeAll();
            }
        }
        currentTimeScale = timeScale;
    }
    #endregion TIME SCALE

    #region TELEKINESIS
    public void SetTelekinesisVelocity(Vector3 velocity)
    {
        _rigidbody.linearVelocity = velocity;
    }

    public void StartTelekinesisMoving()
    {
        isMovingByTelekinesis = true;
        useGravity = false;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.mass = defaultMass;
    }

    internal void StopTelekinesisMoving(float maximumThrowSpeed)
    {
        isMovingByTelekinesis = false;
        useGravity = true;
        var throwVelocity = Vector3.ClampMagnitude(_rigidbody.linearVelocity, maximumThrowSpeed);
        if (isTimeFrozen)
        {
            FreezeAll();
            SetUnbiasedVelocity(throwVelocity);
        }
        else
        {
            FreeFromConstraints();
            _rigidbody.mass = defaultMass / (currentTimeScale * currentTimeScale);
            _rigidbody.linearVelocity = throwVelocity * currentTimeScale;
            _rigidbody.angularVelocity = Vector3.zero;
        }
    }
    #endregion TELEKINESIS

    #region GENERAL
    public void FreezeAll()
    {
        constraintsFreezeAllCommandCount++;
        _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
    }

    public void FreeFromConstraints()
    {
        constraintsFreezeAllCommandCount--;
        if (constraintsFreezeAllCommandCount > 0)
        {
            return;
        }
        _rigidbody.constraints = RigidbodyConstraints.None;
    }

    public Vector3 GetCurrentUnbiasedAngularVelocity()
    {
        if (isTimeFrozen)
        {
            return unbiasedTimeFreezeAngularVelocity;
        }
        return _rigidbody.angularVelocity / currentTimeScale;
    }
    public Vector3 GetCurrentUnbiasedVelocity()
    {
        if (isTimeFrozen)
        {
            return unbiasedTimeFreezeVelocity;
        }
        return _rigidbody.linearVelocity / currentTimeScale;
    }

    public void SetUnbiasedVelocity(Vector3 unbiasedVelocity)
    {
        if (isTimeFrozen)
        {
            unbiasedTimeFreezeVelocity = unbiasedVelocity;
        }
        else
        {
            _rigidbody.linearVelocity = unbiasedVelocity / currentTimeScale;
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
    #endregion GENERAL

    #region REWIND
    public void SetupForRecording(Vector3 unbiasedVelocity, Vector3 unbiasedAngularVelocity)
    {
        FreeFromConstraints();
        SetUnbiasedVelocity(unbiasedVelocity);
        SetUnbiasedAngularVelocity(unbiasedAngularVelocity);
        isRewinding = false;
    }

    public void SetupForPlayback()
    {
        FreezeAll();
        isRewinding = true;
    }
    #endregion REWIND
}
