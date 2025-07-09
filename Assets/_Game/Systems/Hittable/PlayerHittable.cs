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
        }

        override
        public void OnHit(int damage, Vector3 hitDirection)
        {
            _controller.Hit((int)(damageMultiplier * damage), hitDirection);
        }
    }
}
