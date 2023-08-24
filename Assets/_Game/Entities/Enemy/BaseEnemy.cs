using UnityEngine;
using UnityEngine.AI;
using DarkTonic.MasterAudio;

namespace Enemy
{
    public class BaseEnemy : MonoBehaviour
    {
        private Rigidbody _rigidbody;
        
        [Header("References")]
        public MeshRenderer headMeshRenderer;
        public NavMeshAgent navMeshAgent;
        private EnemyHealthUIPool _healthUIPool;

        [Space(10)]
        [Header("Settings")]
        public int maxHealth = 100;
        
        [Space(10)]
        [Header("Watchers")]
        public int health;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _healthUIPool = FindObjectOfType<EnemyHealthUIPool>();
        }

        private void Start()
        {
            health = maxHealth;
        }

        public void Hit(int damage)
        {
            health -= damage;
            Debug.Log("Hit by projectile");
            MasterAudio.PlaySound3DAtTransformAndForget("EnemyHit", transform);
            if (health <= 0)
            {
                _healthUIPool.OnKillTarget(transform);
                Destroy(gameObject);
                return;
            }
            _healthUIPool.OnHitTarget(this);
        }
        
        public void Rush(float strength)
        {
            Debug.Log("Start Rush");
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            _rigidbody.AddForce(strength * transform.forward, ForceMode.Impulse);
        }
        
        public void StopRush()
        {
            Debug.Log("Stop Rush");
            _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }

        public void SetMaterial(Material material)
        {
            headMeshRenderer.sharedMaterial = material;
        }
        
        private void OnCollisionEnter(Collision other)
        {
            Debug.Log(other.gameObject.name);
        }
    }
}
