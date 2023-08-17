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
        public bool isHitting;
        public bool isRecovering;

        public int damage;
        
        public void CoolDown()
        {
            coolDownTime += Time.deltaTime;
        }
        
        public void OnStartAttack(Transform transform)
        {
            _target = transform;
            OnCharge();
        }
        
        protected virtual void OnCharge()
        {
            isCharging = true;
            Debug.Log("OnCharge:" + name);
        }

        protected virtual void HandleCharging()
        {
            if (isCharging) return; 
            OnHit();
        }

        protected virtual void OnHit()
        {
            Debug.Log("OnHit: " + name);
            isCharging = false;
            isHitting = true;
        }

        protected virtual void HandleHitting()
        {
            if (isHitting) return;
            
            OnRecover();
        }
        protected virtual void OnRecover()
        {
            Debug.Log("OnRecover: " + name);
            isHitting = false;
            isRecovering = true;
        }

        protected virtual void HandleRecovering()
        {
            if (isRecovering) return;
            
            OnStopAttacking();
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

        protected virtual void OnStopAttacking()
        {
            Debug.Log("OnStopAtteck: " + name);
            coolDownTime = 0f;
            _target = null;
            isRecovering = false;
        }
    }
}
