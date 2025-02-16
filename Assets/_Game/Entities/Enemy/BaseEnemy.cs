using System;
using UnityEngine;
using UnityEngine.AI;
using DarkTonic.MasterAudio;
using F3PS.Enemy.UI;
using F3PS.AI.States;
using F3PS.Damage.Take;

namespace F3PS.Enemy
{
    public class BaseEnemy : MonoBehaviour
    {
        public Rigidbody body;

        [Header("References")] public GameObject shield;
        public Hittable[] _hittables;
        public MeshRenderer meshRenderer;
        public NavMeshAgent navMeshAgent;
        public PatrolManager patrolManager;
        public TimeObject timeObject;
        public float ScaledDeltaTime => timeObject.ScaledDeltaTime;
        public float TimeScale => timeObject.currentTimeScale;
        protected EnemyHealthUIPool _healthUIPool;
        public bool HasPatrolRoute { get; private set; }

        [SerializeField] protected EnemyStateManager _stateManager;
        public EnemyStateManager StateManager => _stateManager;


        [Space(10)]
        [Header("Settings")]
        public int maxHealth = 100;
        public float moveSpeed;
        
        [Space(10)]
        [Header("Watchers")]
        public bool isActive = true;
        public int health;

        private bool _isDead = false;
        public bool IsDead => _isDead;
        
        public void Activate()
        {
            isActive = true;
            
            navMeshAgent.enabled = true;
            _stateManager.sensorController.gameObject.SetActive(true);
            _stateManager.enabled = true;
            foreach (var hittable in _hittables)
            {
                hittable.enabled = true;
            }
            Initialize();
        }
        
        protected void Awake()
        {
            _healthUIPool = FindObjectOfType<EnemyHealthUIPool>();
        }

        private void Start()
        {
            if (!isActive) return;
            
            Initialize();
        }

        protected void Initialize()
        {
            health = maxHealth;
            HasPatrolRoute = patrolManager != null;
            if (HasPatrolRoute)
            {
                patrolManager.Init();
            }
            _stateManager.Initialize();
        }
        
        private void FixedUpdate()
        {
            if (!isActive) return;
            
            _stateManager.OnPhysicsUpdate();
        }
        
        private void Update()
        {
            if (!isActive) return;
            
            _stateManager.OnFrameUpdate();
        }

        public virtual void Hit(int damage)
        {
            if (_stateManager.IsDying())
            {
                return;
            }
            health -= damage;
            Debug.Log("Took " + damage + " damage");
            MasterAudio.PlaySound3DAtTransformAndForget("Hit", body.transform);
            if (health <= 0)
            {
                _healthUIPool.OnKillTarget(body.transform);
                _isDead = true;
                _stateManager.SwitchState(StateType.DYING);
                return;
            }
            _healthUIPool.OnHitTarget(this);
            _stateManager.Hit();
        }

        public virtual void SetMaterial(Material material)
        {
            if (meshRenderer)
            {
                meshRenderer.sharedMaterial = material;
            }
        }

        public void Deactivate()
        {
            isActive = false;
            navMeshAgent.enabled = false;
            _stateManager.sensorController.gameObject.SetActive(false);
            _stateManager.enabled = false;
            foreach (var hittable in _hittables)
            {
                hittable.enabled = false;
            }
        }
    }
}
