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
        public bool isCharging;
        public Material chargeMaterial;
        public float chargeTimer;
        public float attackTimer;
        public float recoverTimer;
        
        public float chargeStrength;
        public float recoverStrength;
        
        [Space(10)]
        [Header("Rush Watchers")]
        private bool _wasEarlyHit;
        public float chargeTime;
        public float attackTime;
        public float recoverTime;
        public float attackDistance;

        override 
        protected void Initialize()
        {
            _enemyTransform = enemy.body.transform;
            _hitCollider = GetComponent<Collider>();
            _hitCollider.enabled = false;
            _hitBox = GetComponent<HitBox>();
            _hitBox.attackerId = enemy.GetInstanceID();
        }

        override
        public void OnPhysicsUpdate()
        {
            if (isCharging)
            {
                HandleCharging();
            }
            else  base.OnPhysicsUpdate();
        }
        
        override
        public void OnStartAttack(Hittable hittable)
        {
            base.OnStartAttack(hittable);
            _hitCollider.enabled = true;
            OnCharge();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (_wasEarlyHit) return;
            
            var hittable = other.gameObject.GetComponent<Hittable>();
            if (hittable is EnemyHittable or BossHittable)
            {
                return;
            }
            
            _wasEarlyHit = true;
            if (hittable != null && hittable.hittableId != _hitBox.attackerId)
            {
                hittable.OnHit(_hitBox);
            }
        }

        override
        protected void OnStopAttacking()
        {
            _hitCollider.enabled = false;
            enemy.navMeshAgent.isStopped = false;
            
            base.OnStopAttacking();
        }
        
        private void OnCharge()
        {
            enemy.SetMaterial(chargeMaterial);
            isCharging = true;
            _wasEarlyHit = false;
            chargeTime = 0f;
            attackTime = 0f;
            recoverTime = 0f;
            
            _chargeStartPosition = _enemyTransform.position;
            _chargeForward = _enemyTransform.forward;
            _chargeEndPosition = _chargeStartPosition - _chargeForward * chargeStrength;
        }
        
        override
        protected void OnAttack()
        {
            base.OnAttack();

            isAttacking = true;
            _hitCollider.enabled = true;
            _attackStartPosition = _enemyTransform.position;
            _attackForward = _enemyTransform.forward;
            _attackEndPosition = _attackStartPosition + _attackForward * attackDistance;
            
            MasterAudio.PlaySound3DAtTransformAndForget("Enemy_dash", _enemyTransform);
        }
        
        override
        protected void OnRecover()
        {
            base.OnRecover();
            
            isRecovering = true;
            _hitCollider.enabled = false;
            _recoverStartPosition = _enemyTransform.position;
            _recoverForward = _enemyTransform.forward;
            var strength = (_wasEarlyHit ? 1f : 0.5f) * this.recoverStrength;
            _recoverEndPosition = _recoverStartPosition - _recoverForward * strength;
        }
            
        private void HandleCharging()
        {
            chargeTime += enemy.ScaledDeltaTime;
            isCharging = chargeTime < chargeTimer;
            if (!isCharging)
            {
                OnAttack();
            }
            
            _enemyTransform.position = Vector3.Lerp(_chargeStartPosition, _chargeEndPosition, chargeTime / chargeTimer);
        }
        override
        protected void HandleAttack()
        {
            if (_wasEarlyHit)
            {
                isAttacking = false;
            }
            else
            {
                attackTime += enemy.ScaledDeltaTime;
                isAttacking = attackTime < attackTimer;
                _enemyTransform.position = Vector3.Lerp(_attackStartPosition, _attackEndPosition, attackTime / attackTimer);
            }
            
            if (!isAttacking)
            {
                OnRecover();
            }
        }
        
        override
        protected void HandleRecovering()
        {
            recoverTime += enemy.ScaledDeltaTime;
            isRecovering = recoverTime < recoverTimer;
            _enemyTransform.position = Vector3.Slerp(_recoverStartPosition, _recoverEndPosition, recoverTime / recoverTimer);
            isRecovering = recoverTime < recoverTimer;
            
            if (!isRecovering)
            {
                OnStopAttacking();
            }
        }
        
        override
        public bool CanAttack(Hittable hittable)
        {
            if (!base.CanAttack(hittable)) return false;
            
            Transform enemyTransform = enemy.body.transform;
            var targetForward = (hittable.Center() - enemyTransform.position).normalized;
            var actualForward = enemyTransform.forward;
            bool isAlignedWithTarget = Helper.IsOrientedOnXZ(actualForward, targetForward, 0.01f);
            return isAlignedWithTarget;
        }
    }
}
