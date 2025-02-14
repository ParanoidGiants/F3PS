using Cinemachine;
using F3PS.AI.States.Action;
using StarterAssets;
using UnityEngine;

namespace F3PS.Damage.Take
{
    public class PlayerHittable : Hittable
    {
        private ThirdPersonController _controller;
        public CinemachineImpulseSource shakeSource;
        public float shakePower;
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
            if (projectile && !projectile.Hit)
            {
                projectile.SetHit();
                _controller.Hit((int)(damageMultiplier * projectile.damage));
                return;
            }

            // Hit by rush
            var rush = hitBy.gameObject.GetComponent<Rush>();
            if (rush)
            {
                _controller.Hit((int)(damageMultiplier * rush.damage));
                shakeSource.GenerateImpulse(shakePower);
            }
        }
    }
}
