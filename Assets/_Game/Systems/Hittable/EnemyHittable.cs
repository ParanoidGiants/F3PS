using F3PS.AI.States;
using F3PS.AI.States.Action;
using F3PS.Enemy;
using StarterAssets;
using UnityEngine;

namespace F3PS.Damage.Take
{
    public class EnemyHittable : Hittable
    {
        public BaseEnemy enemy;

        void Awake()
        {
            _collider = GetComponent<Collider>();
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
        public void OnHit(int damage, Vector3 hitDirection)
        {
            if (enemy.IsDead)
            {
                return;
            }
            enemy.Hit((int)(damageMultiplier * damage));
            OnHitByPlayer(hitDirection);
        }

        private void OnHitByPlayer(Vector3 hitDirection)
        {
            if (enemy.StateManager.IsAggressive() || enemy.IsDead) return;

            enemy.navMeshAgent.destination = enemy.navMeshAgent.transform.position - hitDirection;
            enemy.StateManager.SwitchState(StateType.CHECKING);
        }
    }
}
