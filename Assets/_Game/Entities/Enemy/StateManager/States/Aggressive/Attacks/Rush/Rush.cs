using System;
using DarkTonic.MasterAudio;
using F3PS.Damage.Take;
using UnityEngine;

namespace F3PS.AI.States.Action
{
    public class Rush : Attack
    {
        private Vector3 _chargeStartPosition;
        private Vector3 _chargeEndPosition;
        private Vector3 _chargeForward;
        private Vector3 _attackStartPosition;
        private Vector3 _attackEndPosition;
        private Vector3 _attackForward;
        private Vector3 _recoverStartPosition;
        private Vector3 _recoverEndPosition;
        private Vector3 _recoverForward;
        
        private Collider _hitCollider;
        private Transform _enemyTransform;
        private HitBox _hitBox;
        
        [Space(10)]
        [Header("Rush Settings")]
        public float chargeTimer;
        public float attackTimer;
        public float recoverTimer;
        
        public float chargeStrength;
        public float recoverStrength;
        public Collider bodyCollider;
        
        [Space(10)]
        [Header("Rush Watchers")]
        public bool wasEarlyHit;
        public float chargeTime;
        public float attackTime;
        public float recoverTime;

        private void Start()
        {
            _enemyTransform = enemy.body.transform;
            _hitCollider = GetComponent<Collider>();
            _hitCollider.enabled = false;
            _hitBox = GetComponent<HitBox>();
            _hitBox.attackerId = enemy.GetInstanceID();
        }
        
        override
        protected void OnCharge()
        {
            wasEarlyHit = false;
            chargeTime = 0f;
            attackTime = 0f;
            recoverTime = 0f;
            
            _chargeStartPosition = _enemyTransform.position;
            _chargeForward = _enemyTransform.forward;
            _chargeEndPosition = _chargeStartPosition - _chargeForward * chargeStrength;
            
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
        protected void OnAttack()
        {
            _hitCollider.enabled = true;
            _attackStartPosition = _enemyTransform.position;
            _attackForward = _enemyTransform.forward;
            _attackEndPosition = _attackStartPosition + _attackForward * attackDistance;
            
            MasterAudio.PlaySound3DAtTransformAndForget("Enemy_dash", _enemyTransform);
            base.OnAttack();
        }

        override
        protected void HandleAttack()
        {
            if (wasEarlyHit)
            {
                Debug.Log("EARLY HIT!");
                attackTime = attackTimer;
                OnRecover();
                return;
            }
            attackTime += Time.deltaTime;
            isAttacking = attackTime < attackTimer;
            _enemyTransform.position = Vector3.Lerp(_attackStartPosition, _attackEndPosition, attackTime / attackTimer);
            base.HandleAttack();
        }
        
        override
        protected void OnRecover()
        {
            base.OnRecover();
            enemy.StopRush();
            _hitCollider.enabled = false;
            _recoverStartPosition = _enemyTransform.position;
            _recoverForward = _enemyTransform.forward;
            var _recoverStrength = (wasEarlyHit ? 1f : 0.5f) * recoverStrength;
            _recoverEndPosition = _recoverStartPosition - _recoverForward * _recoverStrength;
        }
        
        override
        protected void HandleRecovering()
        {
            recoverTime += Time.deltaTime;
            isRecovering = recoverTime < recoverTimer;
            _enemyTransform.position = Vector3.Slerp(_recoverStartPosition, _recoverEndPosition, recoverTime / recoverTimer);
            base.HandleRecovering();
        }

        override
        protected void OnStopAttacking()
        {
            bodyCollider.enabled = true;
            _hitCollider.enabled = false;
            base.OnStopAttacking();
        }

        private void OnTriggerEnter(Collider other)
        {
            var hittable = other.gameObject.GetComponent<Hittable>();
            if (hittable != null && hittable.hittableId != _hitBox.attackerId)
            {
                hittable.OnHit(_hitBox);
            }
        }
    }
}
