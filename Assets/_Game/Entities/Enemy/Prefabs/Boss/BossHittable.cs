using F3PS.Damage.Take;
using F3PS.Enemy;
using UnityEngine;

public class BossHittable : Hittable
{
    public BossEnemy boss;
    void Awake()
    {
        _collider = GetComponent<Collider>();
        hittableId = boss.GetInstanceID();
    }

    override
    public void OnHit(HitBox hitBy)
    {
        // Hit by projectile
        var projectile = hitBy.GetComponent<BaseProjectile>();
        if (projectile && projectile.isPlayer && !projectile.Hit)
        {
            Debug.Log("Boss' " + name + " hit by projectile from " + hitBy.name);
            boss.Hit((int)(damageMultiplier * projectile.damage));
        }
    }
}
