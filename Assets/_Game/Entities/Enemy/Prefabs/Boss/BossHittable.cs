using F3PS.Damage.Take;
using F3PS.Enemy;
using UnityEngine;

public class BossHittable : Hittable
{
    public BossEnemy boss;
    void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    override
    public void OnHit(int damage, Vector3 hitDirection)
    {
        boss.Hit((int)(damageMultiplier * damage));
    }
}
