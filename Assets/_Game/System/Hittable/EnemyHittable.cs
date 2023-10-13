using F3PS.AI.States.Action;
using F3PS.Enemy;
using UnityEngine;

namespace F3PS.Damage.Take
{
    public class EnemyHittable : Hittable
    {
        public BaseEnemy enemy;
        void Awake()
        {
            _collider = GetComponent<Collider>();
            hittableId = enemy.GetInstanceID();
        }

        override
        public void OnHit(HitBox hitBy)
        {
            // Hit by projectile
            var projectile = hitBy.gameObject.GetComponent<BaseProjectile>();
            if (projectile && !projectile.Hit)
            {
                Debug.Log("Hit by projectile: " + hitBy.name);
                enemy.Hit((int)(damageMultiplier * projectile.damage));
                return;
            }
            
            // Hit by rush
            var rush = hitBy.gameObject.GetComponent<Rush>();
            if (rush)
            {
                enemy.Hit((int)(damageMultiplier * rush.damage));
                rush.wasEarlyHit = true;
            }
        }
    }
}
