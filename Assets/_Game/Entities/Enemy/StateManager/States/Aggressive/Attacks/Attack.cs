using F3PS.Damage.Take;
using F3PS.Enemy;
using UnityEngine;

namespace F3PS.AI.States.Action
{
    public abstract class Attack : MonoBehaviour
    {
        [SerializeField] protected Hittable _target;
        
        [Header("General Watchers")]
        public bool isActive;
        public bool isCharging;
        public bool isAttacking;
        public bool isRecovering;
        
        [Header("General References")]
        public BaseEnemy enemy;
        public Material chargeMaterial;
        public Material hitMaterial;
        public Material recoverMaterial;
        private Material _aggressiveMaterial;

        [Header("General Attack Settings")]
        public AttackType type;
        public float stoppingDistanceStay;
        public float stoppingDistanceFollow;
        public float coolDownTime;
        public float coolDownTimer;
        public float attackDistance;
        public int damage;

        public virtual void Init(Material aggressiveMaterial)
        {
            _aggressiveMaterial = aggressiveMaterial;
        }
        
        public void CoolDown()
        {
            coolDownTime += enemy.ScaledDeltaTime;
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
            OnAttack();
        }

        protected virtual void OnAttack()
        {
            enemy.SetMaterial(hitMaterial);
            isCharging = false;
            isAttacking = true;
        }

        protected virtual void HandleAttack()
        {
            if (isAttacking) return;
            
            OnRecover();
        }
        protected virtual void OnRecover()
        {
            enemy.SetMaterial(recoverMaterial);
            isAttacking = false;
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
            else if (isAttacking)
            {
                HandleAttack();
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
            isRecovering = false;
            enemy.SetMaterial(_aggressiveMaterial);
            _target = null;
        }

        public virtual bool CanAttack(Vector3 targetPosition)
        {
            return false;
        }
    }
}
