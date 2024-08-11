
using System;
using UnityEngine;

namespace F3PS.AI.States
{
    [Serializable]
    public class EnabledPhysics : State
    {

        private Rigidbody _rb;
        [SerializeField] private float _physicsEnabledTime;
        [SerializeField] private float _physicsEnabledTimer = 2f;
        private bool _enabled;

        public void Start()
        {
            _rb = enemy.body;
        }
        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void OnPhysicsUpdate()
        {
            base.OnPhysicsUpdate();
            _physicsEnabledTime -= enemy.ScaledDeltaTime;

        }

        public void EnablePhysics()
        {
            _rb.isKinematic = false;
            _enabled = true;
        }

        public void DisablePhysics()
        { 
            _rb.isKinematic = true;
            _enabled = false;
        }
    }
}
