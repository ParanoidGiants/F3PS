using F3PS;
using System;
using UnityEngine;

public class RigidbodyHub : MonoBehaviour
{
    private const double TIME_SCALE_TOLERANCE = 0.001f;
    public Rigidbody _rigidbody;

    [Header("Khonsu Sphere Settings")]
    public bool useGravity = true;
    public float defaultMass;
    public float gravityScale = 1.0f;
    public float currentTimeScale = 1.0f;
    public bool isTimeFrozen = false;

    [Space(10)]
    [Header("ThotMind Settings")]
    public bool isMovingByThotMind = false;

    [Space(10)]
    [Header("AnubisScroll Settings")]
    public bool isAnubisScrolling = false;

    [Space(10)]
    [Header("Watchers")]
    public int setKinematicCount = 0;
    public Vector3 unbiasedTimeFreezeAngularVelocity;
    public Vector3 unbiasedTimeFreezeVelocity;
    internal Vector3 Position => _rigidbody.position;
    internal Quaternion Rotation => _rigidbody.rotation;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.useGravity = false;
        defaultMass = _rigidbody.mass;
    }

    void FixedUpdate()
    {
        if (gravityScale == 0f || !useGravity || isTimeFrozen)
        {
            return;
        }
        var force = Physics.gravity * (currentTimeScale * currentTimeScale * gravityScale);
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
                if (!isMovingByThotMind && !isAnubisScrolling)
                {
                    UnsetKinematic();
                    _rigidbody.linearVelocity = unbiasedTimeFreezeVelocity * timeScale;
                    _rigidbody.angularVelocity = unbiasedTimeFreezeAngularVelocity * timeScale;
                }
            }
            else
            {
                if (!isMovingByThotMind)
                {
                    _rigidbody.linearVelocity *= relation;
                    _rigidbody.angularVelocity *= relation;
                }
            }
            if (!isMovingByThotMind)
            {
                _rigidbody.mass = defaultMass / (timeScale * timeScale);
            }
        }
        else if (!isTimeFrozen)
        {
            isTimeFrozen = true;
            if (!isMovingByThotMind)
            {
                unbiasedTimeFreezeVelocity = _rigidbody.linearVelocity / currentTimeScale;
                unbiasedTimeFreezeAngularVelocity = _rigidbody.angularVelocity / currentTimeScale;
                SetKinematic();
            }
        }
        currentTimeScale = timeScale;
    }
    #endregion TIME SCALE

    #region TELEKINESIS
    public void SetThotMindVelocity(Vector3 velocity)
    {
        _rigidbody.linearVelocity = velocity;
    }

    public void StartThotMindMoving()
    {
        isMovingByThotMind = true;
        useGravity = false;
        _rigidbody.isKinematic = false;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.mass = defaultMass;
    }

    internal void StopThotMindMoving(float maximumThrowSpeed)
    {
        isMovingByThotMind = false;
        useGravity = true;
        _rigidbody.constraints = RigidbodyConstraints.None;
        var throwVelocity = Vector3.ClampMagnitude(_rigidbody.linearVelocity, maximumThrowSpeed);
        if (isTimeFrozen)
        {
            SetKinematic();
            SetUnbiasedVelocity(throwVelocity);
        }
        else
        {
            UnsetKinematic();
            _rigidbody.mass = defaultMass / (currentTimeScale * currentTimeScale);
            _rigidbody.linearVelocity = throwVelocity * currentTimeScale;
            _rigidbody.angularVelocity = Vector3.zero;
        }
    }
    #endregion TELEKINESIS

    #region GENERAL
    public void SetKinematic()
    {
        setKinematicCount++;
        _rigidbody.isKinematic = true;
    }

    public void UnsetKinematic()
    {
        setKinematicCount--;
        if (setKinematicCount > 0)
        {
            return;
        }
        _rigidbody.isKinematic = false;
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
        if (isMovingByThotMind)
        {
            return Vector3.ClampMagnitude(
                _rigidbody.linearVelocity,
                GameManager.Instance.PlayerData.ThotMindSkillData.MaximumThrowSpeed
            ) / currentTimeScale;
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
        UnsetKinematic();
        SetUnbiasedVelocity(unbiasedVelocity);
        SetUnbiasedAngularVelocity(unbiasedAngularVelocity);
        isAnubisScrolling = false;
    }

    public void SetupForPlayback()
    {
        SetKinematic();
        isAnubisScrolling = true;
    }

    public void MovePosition(Vector3 position)
    {
        _rigidbody.MovePosition(position);
    }

    public void MoveRotation(Quaternion quaternion)
    {
        _rigidbody.MoveRotation(quaternion);
    }
    #endregion REWIND
}
