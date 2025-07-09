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
            _controller = FindFirstObjectByType<ThirdPersonController>();
            _collider = GetComponent<Collider>();
            _hittableId = _controller.GetInstanceID();
        }

        override
        public void OnHit(HitBox hitBy, Vector3 hitDirection)
        {
            var damage = (int)(damageMultiplier * hitBy.damage);
            _controller.Hit(damage, hitDirection);
        }
    }
}
