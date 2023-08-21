using UnityEngine;

namespace Enemy.States
{
    public class Rush : Attack
    {
        private Vector3 _chargeStartPosition;
        private Vector3 _chargeEndPosition;
        private Vector3 _chargeForward;
        
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
            enemy = navMeshAgent.GetComponent<BaseEnemy>();
        }
        
        private void EarlyHit()
        {
            OnRecover();
            wasEarlyHit = true;
        }

        override
        protected void OnCharge()
        {
            chargeTime = 0f;
            hitTime = 0f;
            recoverTime = 0f;
            
            _chargeStartPosition = navMeshAgent.transform.position;
            _chargeForward = navMeshAgent.transform.forward;
            _chargeEndPosition = _chargeStartPosition - _chargeForward * 0.5f;
            
            base.OnCharge();
        }

        override
        protected void HandleCharging()
        {
            chargeTime += Time.deltaTime;
            isCharging = chargeTime < chargeTimer;
            navMeshAgent.transform.position = Vector3.Lerp(_chargeStartPosition, _chargeEndPosition, chargeTime / chargeTimer);
            base.HandleCharging();
        }
        
        override
        protected void OnHit()
        {
            wasEarlyHit = false;
            enemy.Rush(rushStrength, damage, () => EarlyHit());
            base.OnHit();
        }

        override
        protected void HandleHitting()
        {
            if (wasEarlyHit)
            {
                hitTime = hitTimer;
                return;
            }
            hitTime += Time.deltaTime;
            isHitting = hitTime < hitTimer;
            base.HandleHitting();
        }

        private Vector3 _recoverStartPosition;
        private Vector3 _recoverEndPosition;
        private Vector3 _recoverForward;
        override
        protected void OnRecover()
        {
            base.OnRecover();
            enemy.StopRush();
            var enemyTransform = enemy.transform;
            _recoverStartPosition = enemyTransform.position;
            _recoverForward = enemyTransform.forward;
            _recoverEndPosition = _recoverStartPosition - _recoverForward * 0.2f;
        }
        
        override
        protected void HandleRecovering()
        {
            recoverTime += Time.deltaTime;
            var newPositioon = Vector3.Lerp(_recoverStartPosition, _recoverEndPosition, recoverTime / recoverTimer);
            enemy.transform.position = newPositioon;
            _recoverStartPosition = enemy.transform.position;
            isRecovering = recoverTime < recoverTimer;
            base.HandleRecovering();
        }
    }
}
