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
                if (health > 0)
                {
                    FindObjectOfType<EnemyHealthUIPool>().OnHitTarget(this);
                }
                else
                {
                    FindObjectOfType<EnemyHealthUIPool>().OnKillTarget(transform);
                    Destroy(gameObject);
                }
            }

            if (isRushing && Helper.IsLayerPlayerLayer(other.gameObject.layer))
            {
                other.gameObject.GetComponent<ThirdPersonController>().Hit(damage);
            }
        }

        public void Rush()
        {
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            navMeshAgent.enabled = false;
            isRushing = true;
            _rigidbody.AddForce(10f * transform.forward + transform.up, ForceMode.Impulse);
        }
        public void StopRush()
        {
            _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            navMeshAgent.enabled = true;
            isRushing = false;
        }

        public bool HasReachedDestination()
        {
            return !navMeshAgent.pathPending 
                   && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance 
                   && (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f);
        }

    }
}
