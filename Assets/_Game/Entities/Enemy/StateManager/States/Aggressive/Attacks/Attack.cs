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
        public Material attackMaterial;
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


        protected virtual void Initialize() { }

        public virtual void OnStartAttack(Hittable hittable)
        {
            isActive = true;
            _target = hittable;
            OnCharge();
        }
        protected virtual void OnCharge()
        {
            enemy.SetMaterial(chargeMaterial);
        }
        protected virtual void OnAttack()
        {
            enemy.SetMaterial(attackMaterial);
        }
        protected virtual void OnRecover()
        {
            enemy.SetMaterial(recoverMaterial);
        }
        
        public virtual void OnUpdate() { }
        protected virtual void HandleCharging() { }
        protected virtual void HandleAttack() { }
        protected virtual void HandleRecovering() { }
        
        public virtual void Initialize(Material aggressiveMaterial)
        {
            _aggressiveMaterial = aggressiveMaterial;
            Initialize();
        }
        
        protected virtual void OnStopAttacking()
        {
            _target = null;
            isActive = false;
            coolDownTime = 0f;
            enemy.SetMaterial(_aggressiveMaterial);
        }
        
        public void CoolDown()
        {
            coolDownTime += enemy.ScaledDeltaTime;
        }
        
        
        public bool HasCooledDown()
        {
            return coolDownTime >= coolDownTimer;
        }

        public virtual bool CanAttack(Vector3 targetPosition)
        {
            return HasCooledDown();
        }
    }
}
