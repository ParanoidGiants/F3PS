using DG.Tweening;
using F3PS.AI.States.Action;
using StarterAssets;
using UnityEngine;

namespace F3PS.Damage.Take
{
    public class PlayerHittable : Hittable
    {
        private ThirdPersonController _controller;
        [Header("References")]
        public HittableManager hittableManager;
        
        void Awake()
        {
            _controller = FindObjectOfType<ThirdPersonController>();
            _collider = GetComponent<Collider>();
            hittableId = _controller.GetInstanceID();
        }

        override
        public void OnHit(HitBox hitBy)
        {
            // Hit by projectile
            var projectile = hitBy.gameObject.GetComponent<BaseProjectile>();
            var damage = 0;
            if (projectile && !projectile.Hit)
            {
                damage = (int)(damageMultiplier * projectile.damage);
                projectile.SetHit();
            }

            // Hit by rush
            var rush = hitBy.gameObject.GetComponent<Rush>();
            if (rush)
            {
                damage = (int)(damageMultiplier * rush.damage);
            }
            _controller.Hit(damage);
            hittableManager.Shake(damage);
            hittableManager.Flash();
        }
    }
}
