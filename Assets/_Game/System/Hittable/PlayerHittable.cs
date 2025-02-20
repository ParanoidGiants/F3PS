using F3PS.AI.States.Action;
using StarterAssets;
using System;
using UnityEngine;

namespace F3PS.Damage.Take
{
    public class PlayerHittable : Hittable
    {
        private ThirdPersonController _controller;

        public Action<Hittable> OnDestroyed;

        private void OnDestroy()
        {
            OnDestroyed?.Invoke(this);
        }

        void Awake()
        {
            _controller = FindObjectOfType<ThirdPersonController>();
            _collider = GetComponent<Collider>();
            _hittableId = _controller.GetInstanceID();
        }

        override
        public void OnHit(HitBox hitBy, Vector3 hitDirection)
        {
            // Hit by projectile
            var projectile = hitBy.gameObject.GetComponent<BaseProjectile>();
            var damage = 0;
            if (projectile)
            {
                damage = (int)(damageMultiplier * projectile.damage);
            }

            // Hit by rush
            var rush = hitBy.gameObject.GetComponent<Rush>();
            if (rush)
            {
                damage = (int)(damageMultiplier * rush.damage);
            }
            _controller.Hit(damage, hitDirection);
        }
    }
}
