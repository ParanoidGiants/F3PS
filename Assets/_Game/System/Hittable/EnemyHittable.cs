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
        protected void OnHit(HitBox hitBy)
        {
            // Hit by projectile
            var projectile = hitBy.gameObject.GetComponent<Projectile>();
            if (projectile && !projectile.Hit)
            {
                projectile.SetHit();
                enemy.Hit((int)(damageMultiplier * projectile.damage));
                return;
            }
            
            // Hit by projectile
            var rush = hitBy.gameObject.GetComponent<Rush>();
            if (rush)
            {
                enemy.Hit((int)(damageMultiplier * rush.damage));
                rush.wasEarlyHit = true;
                return;
            }
        }
    }
}
