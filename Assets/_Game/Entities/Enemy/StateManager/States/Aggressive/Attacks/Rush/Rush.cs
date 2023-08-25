using System;
using UnityEngine;

namespace F3PS.AI.States.Action
{
    public class Rush : Attack
    {
        private Vector3 _chargeStartPosition;
        private Vector3 _chargeEndPosition;
        private Vector3 _chargeForward;
        private Vector3 _recoverStartPosition;
        private Vector3 _recoverEndPosition;
        private Vector3 _recoverForward;
        
        private Collider _collider;
        private Transform _enemyTransform;
        
        [Space(10)]
        [Header("Rush Settings")]
        public float rushStrength;
        public float chargeTimer;
        public float hitTimer;
        public float recoverTimer;
        
        [Space(10)]
        [Header("Rush Watchers")]
        public bool wasEarlyHit;
        public float chargeTime;
        public float hitTime;
        public float recoverTime;

        private void Start()
        {
            _enemyTransform = enemy.transform;
            _collider = GetComponent<Collider>();
            _collider.enabled = false;
        }
        
        override
        protected void OnCharge()
        {
            wasEarlyHit = false;
            chargeTime = 0f;
            hitTime = 0f;
            recoverTime = 0f;
            
            _chargeStartPosition = _enemyTransform.position;
            _chargeForward = _enemyTransform.forward;
            _chargeEndPosition = _chargeStartPosition - _chargeForward * 0.5f;
            
            base.OnCharge();
        }

        override
        protected void HandleCharging()
        {
            chargeTime += Time.deltaTime;
            isCharging = chargeTime < chargeTimer;
            _enemyTransform.position = Vector3.Lerp(_chargeStartPosition, _chargeEndPosition, chargeTime / chargeTimer);
            base.HandleCharging();
        }
        
        override
        protected void OnHit()
        {
            _collider.enabled = true;
            enemy.Rush(rushStrength);
            base.OnHit();
        }

        override
        protected void HandleHitting()
        {
            if (wasEarlyHit)
            {
                hitTime = hitTimer;
                OnRecover();
                return;
            }
            hitTime += Time.deltaTime;
            isHitting = hitTime < hitTimer;
            base.HandleHitting();
        }
        
        override
        protected void OnRecover()
        {
            base.OnRecover();
            enemy.StopRush();
            _collider.enabled = false;
            _recoverStartPosition = _enemyTransform.position;
            _recoverForward = _enemyTransform.forward;
            _recoverEndPosition = _recoverStartPosition - _recoverForward * 0.2f;
        }
        
        override
        protected void HandleRecovering()
        {
            recoverTime += Time.deltaTime;
            var newPositioon = Vector3.Lerp(_recoverStartPosition, _recoverEndPosition, recoverTime / recoverTimer);
            _enemyTransform.position = newPositioon;
            _recoverStartPosition = _enemyTransform.position;
            isRecovering = recoverTime < recoverTimer;
            base.HandleRecovering();
        }

        override
        protected void OnStopAttacking()
        {
            _collider.enabled = false;
            base.OnStopAttacking();
        }

        private void OnCollisionEnter(Collision other)
        {
            Debug.Log(other.gameObject.name);
        }
    }
}
