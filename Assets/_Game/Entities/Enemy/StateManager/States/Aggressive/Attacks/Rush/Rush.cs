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
        
        private Collider _hitCollider;
        private Transform _enemyTransform;
        private HitBox _hitBox;
        
        [Space(10)]
        [Header("Rush Settings")]
        public float rushStrength;
        public float chargeTimer;
        public float hitTimer;
        public float recoverTimer;
        public Collider bodyCollider;
        
        [Space(10)]
        [Header("Rush Watchers")]
        public bool wasEarlyHit;
        public float chargeTime;
        public float hitTime;
        public float recoverTime;

        private void Start()
        {
            _enemyTransform = enemy.transform;
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
            _hitCollider.enabled = true;
            bodyCollider.enabled = false;
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
            _hitCollider.enabled = false;
            _recoverStartPosition = _enemyTransform.position;
            _recoverForward = _enemyTransform.forward;
            _recoverEndPosition = _recoverStartPosition - _recoverForward;
        }
        
        override
        protected void HandleRecovering()
        {
            recoverTime += Time.deltaTime;
            var newPosition = Vector3.Lerp(_recoverStartPosition, _recoverEndPosition, recoverTime / recoverTimer);
            _enemyTransform.position = newPosition;
            _recoverStartPosition = _enemyTransform.position;
            isRecovering = recoverTime < recoverTimer;
            base.HandleRecovering();
        }

        override
        protected void OnStopAttacking()
        {
            bodyCollider.enabled = true;
            _hitCollider.enabled = false;
            base.OnStopAttacking();
        }
    }
}
