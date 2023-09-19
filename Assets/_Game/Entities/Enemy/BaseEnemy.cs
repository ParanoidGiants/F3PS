using UnityEngine;
using UnityEngine.AI;
using DarkTonic.MasterAudio;
using F3PS.Enemy.UI;

namespace F3PS.Enemy
{
    public class BaseEnemy : MonoBehaviour
    {
        public Rigidbody body;
        
        [Header("References")]
        public MeshRenderer meshRenderer;
        public NavMeshAgent navMeshAgent;
        public PatrolManager patrolManager;
        private EnemyHealthUIPool _healthUIPool;
        public bool HasPatrolRoute { get; private set; }

        [Space(10)]
        [Header("Settings")]
        public int maxHealth = 100;
        
        [Space(10)]
        [Header("Watchers")]
        public int health;


        private void Awake()
        {
            _healthUIPool = FindObjectOfType<EnemyHealthUIPool>();
            HasPatrolRoute = patrolManager != null;
            if (HasPatrolRoute)
            {
                patrolManager.Init();
            }
        }

        private void Start()
        {
            health = maxHealth;
        }

        public void Hit(int damage)
        {
            health -= damage;
            MasterAudio.PlaySound3DAtTransformAndForget("EnemyHit", body.transform);
            if (health <= 0)
            {
                _healthUIPool.OnKillTarget(body.transform);
                Destroy(gameObject);
                return;
            }
            _healthUIPool.OnHitTarget(this);
        }
        
        public void Rush(float strength)
        {
            body.constraints = RigidbodyConstraints.FreezeRotation;
            body.AddForce(strength * body.transform.forward, ForceMode.Impulse);
        }
        
        public void StopRush()
        {
            body.constraints = RigidbodyConstraints.FreezeAll;
        }

        public void SetMaterial(Material material)
        {
            meshRenderer.sharedMaterial = material;
        }
    }
}
