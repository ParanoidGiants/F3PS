using Enemy.States;
using Player;
using UnityEngine;

public class PlayerHittable : Hittable
{
    public Extensions playerExtensions;
    void Awake()
    {
        _collider = GetComponent<Collider>();

    }

    override
    protected void OnHit(Collider hitBy)
    {
        // Hit by projectile
        var projectile = hitBy.gameObject.GetComponent<Projectile>();
        if (projectile && !projectile.Hit)
        {
            projectile.SetHit();
            playerExtensions.Hit((int)(damageMultiplier * projectile.damage));
        }

        // Hit by rush
        var rush = hitBy.gameObject.GetComponent<Rush>();
        if (rush)
        {
            playerExtensions.Hit((int)(damageMultiplier * rush.damage));
            rush.wasEarlyHit = true;
        }
    }
}
