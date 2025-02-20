using F3PS.AI.States.Action;
using F3PS.Enemy;
using StarterAssets;
using UnityEngine;

namespace F3PS.Damage.Take
{
    public class EnemyHittable : Hittable
    {
        private int _playerId;
        public BaseEnemy enemy;

        void Awake()
        {
            _collider = GetComponent<Collider>();
            _hittableId = enemy.GetInstanceID();
            _playerId = FindObjectOfType<ThirdPersonController>().playerSpace.GetInstanceID();
        }

        private void OnEnable()
        {
            _collider.enabled = true;
        }
        
        private void OnDisable()
        {
            _collider.enabled = false;
        }

        override
        public void OnHit(HitBox hitBy, Vector3 hitDirection)
        {
            if (enemy.IsDead)
            {
                return;
            }
            // Hit by projectile
            var damage = 0;
            var projectile = hitBy.GetComponent<BaseProjectile>();
            if (projectile)
            {
                damage = (int)(damageMultiplier * projectile.damage);
            }
            
            // Hit by rush
            var rush = hitBy.GetComponent<Rush>();
            if (rush)
            {
                damage = (int)(damageMultiplier * rush.damage);
            }
            enemy.Hit(damage);
            if (hitBy.attackerId == _playerId)
            {
                OnHitByPlayer(hitDirection);
            }
        }

        private void OnHitByPlayer(Vector3 hitDirection)
        {
            if (enemy.StateManager.IsAggressive() || enemy.IsDead) return;

            enemy.navMeshAgent.destination = enemy.transform.position - hitDirection;
            enemy.StateManager.SwitchState(StateType.CHECKING);
        }
    }
}
