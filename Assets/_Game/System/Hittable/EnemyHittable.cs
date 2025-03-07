using System;
using F3PS.AI.States.Action;
using F3PS.Enemy;
using UnityEngine;

namespace F3PS.Damage.Take
{
    public class EnemyHittable : Hittable
    {
        public BaseEnemy enemy;
        void Awake()
        {
            _collider = GetComponent<Collider>();
            hittableId = enemy.GetInstanceID();
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
        public void OnHit(HitBox hitBy)
        {
            // Hit by projectile
            var projectile = hitBy.GetComponent<BaseProjectile>();
            if (projectile && !projectile.Hit)
            {
                Debug.Log(enemy.name + " hit by projectile from " + hitBy.name);
                enemy.Hit((int)(damageMultiplier * projectile.damage));
                return;
            }
            
            // Hit by rush
            var rush = hitBy.GetComponent<Rush>();
            if (rush)
            {
                Debug.Log(enemy.name + " hit by rush from " + hitBy.name);
                enemy.Hit((int)(damageMultiplier * rush.damage));
            }
        }

        internal void OnHitByPlayer(Vector3 hitDirection)
        {
            if (enemy.StateManager.IsAggressive()) return;

            Debug.Log(enemy.name + " hit by player");
            enemy.navMeshAgent.destination = enemy.transform.position - hitDirection;
            enemy.StateManager.SwitchState(StateType.CHECKING);
            Debug.Log(hitDirection);
            Debug.DrawRay(transform.position, hitDirection, Color.red, 3f);
        }
    }
}
