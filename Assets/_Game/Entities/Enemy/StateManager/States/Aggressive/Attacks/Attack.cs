using UnityEngine;
using UnityEngine.AI;

namespace Enemy.States
{
    public abstract class Attack : MonoBehaviour
    {
        private Transform _target;
        
        [Space(10)]
        [Header("Base Attack Settings")]
        public AttackType type;

        public NavMeshAgent navMeshAgent;
        public float stoppingDistance;
        
        public float coolDownTime;
        public float coolDownTimer;

        public bool isCharging;
        public float chargeTime;
        public float chargeTimer;
        
        public bool isHitting;
        public float hitTime;
        public float hitTimer;
        
        public bool isRecovering;
        public float recoverTime;
        public float recoverTimer;

        public int damage;
        
        public void CoolDown()
        {
            coolDownTime += Time.deltaTime;
        }
        
        public void OnStartAttack(Transform transform)
        {
            _target = transform;
            chargeTime = 0f;
            hitTime = 0f;
            recoverTime = 0f;
            
            OnCharge();
        }

        public bool HasCooledDown()
        {
            return coolDownTime >= coolDownTimer;
        }

        public void OnUpdate()
        {
            if (isCharging)
            {
                HandleCharging();
            }
            else if (isHitting)
            {
                HandleHitting();
            }
            else if (isRecovering)
            {
                HandleRecovering();
            }
        }

        public void OnCharge()
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.updateRotation = false;
            isCharging = true;
            chargeTime = 0f;
            hitTime = 0f;
            recoverTime = 0f;
            Debug.Log("CHARGE");
        }
        
        private void HandleCharging()
        {
            chargeTime += Time.deltaTime;
            if (chargeTime >= chargeTimer)
            {
                OnHit();
            }
        }

        protected virtual void OnHit()
        {
            navMeshAgent.updateRotation = true;
            isCharging = false;
            isHitting = true;
            Debug.Log("HIT");
        }

        protected virtual void HandleHitting()
        {
            hitTime += Time.deltaTime;
            if (hitTime >= hitTimer)
            {
                OnRecover();
            }
        }

        protected virtual void OnRecover()
        {
            isHitting = false;
            isRecovering = true;
            Debug.Log("RECOVER");
        }
        
        private void HandleRecovering()
        {
            recoverTime += Time.deltaTime;
            if (recoverTime >= recoverTimer)
            {
                OnStopAttacking();
            }
        }

        private void OnStopAttacking()
        {
            coolDownTime = 0f;
            _target = null;
            isRecovering = false;
            navMeshAgent.isStopped = false;
            Debug.Log("DONE!");
        }
    }
}
