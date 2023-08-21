using System;
using StarterAssets;
using UnityEngine;
using UnityEngine.AI;
using DarkTonic.MasterAudio;

namespace Enemy
{
    public class BaseEnemy : MonoBehaviour
    {
        private Rigidbody _rigidbody;
        private ThirdPersonController _player;
        
        [Header("References")]
        public MeshRenderer headMeshRenderer;
        public NavMeshAgent navMeshAgent;
        private EnemyHealthUIPool _healthUIPool;

        [Space(10)]
        [Header("Settings")]
        public int maxHealth = 100;
        private int _damage;
        
        [Space(10)]
        [Header("Watchers")]
        public int health;
        public bool isRushing;
        public Vector3 Velocity => _rigidbody.velocity;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _healthUIPool = FindObjectOfType<EnemyHealthUIPool>();
            _player = FindObjectOfType<ThirdPersonController>();
        }

        private void Start()
        {
            health = maxHealth;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (isRushing && Helper.IsLayerPlayerLayer(other.gameObject.layer))
            {
                _player.extensions.Hit(_damage);
                StopRush();
                _earlyHit();
            }
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

        private Action _earlyHit;
        public void Rush(float strength, int attackDamage, Action earlyHit)
        {
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            _damage = attackDamage;
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
