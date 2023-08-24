using System;
using UnityEngine;

public class Hittable : MonoBehaviour
{
    protected Collider _collider;
    public float damageMultiplier;
    
    void Awake()
    {
        _collider = GetComponent<Collider>();
    }
    
    public Vector3 Center()
    {
        return _collider.bounds.center;
    }

    protected virtual void OnHit(Collider hitBy) {}
    
    private void OnCollisionEnter(Collision other)
    {
        OnHit(other.collider);
    }
}
