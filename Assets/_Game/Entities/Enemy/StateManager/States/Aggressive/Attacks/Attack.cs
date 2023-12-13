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
        public bool isAttacking;
        public bool isRecovering;
        
        [Header("General References")]
        public BaseEnemy enemy;
        public Material attackMaterial;
        public Material recoverMaterial;
        private Material _aggressiveMaterial;

        [Header("General Attack Settings")]
        public AttackType type;
        public float stoppingDistanceStay;
        public float stoppingDistanceFollow;
        public float coolDownTime;
        public float coolDownTimer;
        public int damage;


        protected virtual void Initialize() { }

        public virtual void OnStartAttack(Hittable hittable)
        {
            _target = hittable;
            isActive = true;
            enemy.navMeshAgent.isStopped = true;
        }

        protected virtual void OnAttack()
        {
            enemy.SetMaterial(attackMaterial);
        }

        protected virtual void OnRecover()
        {
            enemy.SetMaterial(recoverMaterial);
        }

        public virtual void OnPhysicsUpdate()
        {
            if (isAttacking)
            {
                HandleAttack();
            }
            else if (isRecovering)
            {
                HandleRecovering();
            }
        }

        public void OnFrameUpdate()
        {
            if (!HasCooledDown())
            {
                CoolDown();
            }
        }
        
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
            enemy.navMeshAgent.isStopped = false;
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

        public virtual bool CanAttack(Hittable hittable)
        {
            return !isActive && HasCooledDown();
        }
    }
}
