using F3PS.Damage.Take;
using F3PS.Enemy;
using UnityEngine;
using UnityEngine.AI;

namespace F3PS.AI.States.Action
{
    public abstract class Attack : MonoBehaviour
    {
        protected Hittable _target;
        [Header("General Watchers")]
        public bool isActive;
        public bool isCharging;
        public bool isHitting;
        public bool isRecovering;
        
        [Header("General References")]
        public Material chargeMaterial;
        public Material hitMaterial;
        public Material recoverMaterial;

        [Header("General Attack Settings")]
        public AttackType type;
        public BaseEnemy enemy;
        public NavMeshAgent navMeshAgent;
        public float stoppingDistanceStay;
        public float stoppingDistanceFollow;
        public float coolDownTime;
        public float coolDownTimer;
        public float attackDistance;
        public int damage;

        public void CoolDown()
        {
            coolDownTime += Time.deltaTime;
        }
        
        public virtual void OnStartAttack(Hittable hittable)
        {
            isActive = true;
            _target = hittable;
            OnCharge();
        }
        
        protected virtual void OnCharge()
        {
            enemy.SetMaterial(chargeMaterial);
            isCharging = true;
        }

        protected virtual void HandleCharging()
        {
            if (isCharging) return; 
            OnHit();
        }

        protected virtual void OnHit()
        {
            enemy.SetMaterial(hitMaterial);
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
            enemy.SetMaterial(recoverMaterial);
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
            coolDownTime = 0f;
            isActive = false;
            _target = null;
            isRecovering = false;
        }

        public virtual bool IsInAttackDistance(Vector3 targetPosition)
        {
            var position = navMeshAgent.transform.position;
            var direction1 = (targetPosition - position).normalized;
            
            if (Physics.Raycast(position, direction1, out var hit, attackDistance, Helper.PlayerLayer))
            {
                if (Physics.Raycast(position, direction1, hit.distance, Helper.DefaultLayer))
                {
                    return false;                
                } 
                return true;
            }
            return false;
        }
    }
}
