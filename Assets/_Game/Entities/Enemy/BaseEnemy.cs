using System;
using StarterAssets;
using UnityEngine;
using UnityEngine.AI;

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
        public int damage = 10;
        
        [Space(10)]
        [Header("Watchers")]
        public int health;
        public bool isRushing;
        public Vector3 Velocity => _rigidbody.velocity;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _healthUIPool = FindObjectOfType<EnemyHealthUIPool>();
        }

        private void Start()
        {
            health = maxHealth;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (Helper.IsLayerProjectileLayer(other.gameObject.layer))
            {
                var projectile = other.gameObject.GetComponent<Projectile>();
                health -= projectile.damage;
                Debug.Log("Hit by projectile");
                if (health <= 0)
                {
                    _healthUIPool.OnKillTarget(transform);
                    Destroy(gameObject);
                    return;
                }
                
                _healthUIPool.OnHitTarget(this);
                GetComponentInChildren<EnemyStateManager>();
                return;
            }

            if (isRushing && Helper.IsLayerPlayerLayer(other.gameObject.layer))
            {
                other.gameObject.GetComponent<ThirdPersonController>().Hit(damage);
                StopRush();
                _earlyHit();
            }
        }

        private Action _earlyHit;
        public void Rush(float strength, int attackDamage, Action earlyHit)
        {
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            damage = attackDamage;
            isRushing = true;
            _rigidbody.AddForce(strength * transform.forward + transform.up, ForceMode.Impulse);
            _earlyHit = earlyHit;
        }
        
        public void StopRush()
        {
            _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            isRushing = false;
        }

        public void SetMaterial(Material material)
        {
            headMeshRenderer.sharedMaterial = material;
        }
    }
}
