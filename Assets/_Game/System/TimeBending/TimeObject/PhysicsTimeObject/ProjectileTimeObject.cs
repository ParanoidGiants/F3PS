using System;
using UnityEngine;

public class ProjectileTimeObject : PhysicsTimeObject
{
    [SerializeField] private TrailRenderer _trail;
    [SerializeField] private TrailRenderer _slowMoTrail;
    private float _baseTrailTime; 
    
    override
    protected void Awake()
    {
        _baseTrailTime = _trail.time;
        base.Awake();
    }

    override
    protected void OnDisable()
    {
        base.OnDisable();
        ClearTrail();
    }

    override
    public void PitchTimeScale(float newTimeScale)
    {
        base.PitchTimeScale(newTimeScale);
        if (newTimeScale < TOLERANCE) return;
        
        if (Math.Abs(newTimeScale - 1f) > TOLERANCE)
        {
            _trail.enabled = false;
            _slowMoTrail.enabled = true;
            _slowMoTrail.time = _baseTrailTime / newTimeScale;
        }
        else
        {
            _trail.enabled = true;
            _slowMoTrail.enabled = false;
        }
    }
    
    public void ClearTrail()
    {
        _trail.Clear();
        _slowMoTrail.Clear();
    }
}
