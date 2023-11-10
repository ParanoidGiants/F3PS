using System;
using UnityEngine;

public class ProjectileTimeObject : PhysicsTimeObject
{
    [SerializeField] private TrailRenderer _trail;
    [SerializeField] private TrailRenderer _slowMoTrail;
    private float _baseTrailTime; 
    
    private void Awake()
    {
        _baseTrailTime = _trail.time;
        base.Awake();
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
    

    private void OnDisable()
    {
        base.OnDisable();
        _trail.Clear();
        _slowMoTrail.Clear();
    }
}
