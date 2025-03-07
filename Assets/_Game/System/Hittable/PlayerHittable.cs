using System.Runtime.CompilerServices;
using F3PS.AI.States.Action;
using Player;
using UnityEngine;

namespace F3PS.Damage.Take
{
    public class PlayerHittable : Hittable
    {
        public Extensions playerExtensions;
        void Awake()
        {
            _collider = GetComponent<Collider>();
            hittableId = playerExtensions.GetInstanceID();
        }

        override
        public void OnHit(HitBox hitBy)
        {
            // Hit by projectile
            var projectile = hitBy.gameObject.GetComponent<BaseProjectile>();
            if (projectile && !projectile.Hit)
            {
                projectile.SetHit();
                playerExtensions.Hit((int)(damageMultiplier * projectile.damage));
                return;
            }

            // Hit by rush
            var rush = hitBy.gameObject.GetComponent<Rush>();
            if (rush)
            {
                playerExtensions.Hit((int)(damageMultiplier * rush.damage));
            }
        }
    }
}
