using UnityEngine;

namespace Enemy.States
{
    public class Rush : Attack
    {
        public float rushStrength;
        public BaseEnemy enemy;
        public bool wasEarlyHit;

        public float chargeTime;
        public float chargeTimer;
        
        public float hitTime;
        public float hitTimer;
        
        public float recoverTime;
        public float recoverTimer;
        
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
            
            navMeshAgent.isStopped = true;
            navMeshAgent.updateRotation = false;
            
            base.OnCharge();
        }

        override
        protected void HandleCharging()
        {
            chargeTime += Time.deltaTime;
            isCharging = chargeTime < chargeTimer;
            base.HandleCharging();
        }
        
        override
        protected void OnHit()
        {
            navMeshAgent.updateRotation = true;
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

        override
        protected void OnRecover()
        {
            base.OnRecover();
            enemy.StopRush();
        }
        
        override
        protected void HandleRecovering()
        {
            recoverTime += Time.deltaTime;
            isRecovering = recoverTime < recoverTimer;
            base.HandleRecovering();
        }
        
        override
        protected void OnStopAttacking()
        {
            navMeshAgent.isStopped = false;
            base.OnStopAttacking();
        }
    }
}
