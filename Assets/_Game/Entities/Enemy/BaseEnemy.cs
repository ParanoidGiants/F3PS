using UnityEngine;
using UnityEngine.AI;
using DarkTonic.MasterAudio;
using F3PS.Enemy.UI;

namespace F3PS.Enemy
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
            Debug.Log("Took " + damage + " damage");
            MasterAudio.PlaySound3DAtTransformAndForget("Hit", transform);
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
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            _rigidbody.AddForce(strength * transform.forward, ForceMode.Impulse);
            MasterAudio.PlaySound3DAtTransformAndForget("Enemy_dash", transform);
        }
        
        public void StopRush()
        {
            _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }

        public void SetMaterial(Material material)
        {
            headMeshRenderer.sharedMaterial = material;
        }
    }
}
