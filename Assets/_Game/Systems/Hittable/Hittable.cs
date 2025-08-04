using System;
using UnityEngine;

public class Hittable : MonoBehaviour
{
    [Header("Reference")]
    public GameObject owner;

    [Header("Settings")]
    public float damageMultiplier;

    public event Action<Hittable> onDisabled;

    protected Collider _collider;
        
    public Vector3 Center()
    {
        return _collider.bounds.center;
    }

    public virtual void OnHit(int damage, Vector3 hitDirection) { }

    public bool IsOwner(int instanceId)
    {
        return owner.GetInstanceID() == instanceId;
    }

    private void OnDisable()
    {
        onDisabled?.Invoke(this);
    }
}
