using F3PS.AI.States;
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
            _playerId = FindFirstObjectByType<ThirdPersonController>().transform.parent.GetInstanceID();
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
            enemy.Hit((int)(damageMultiplier * hitBy.damage));
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
