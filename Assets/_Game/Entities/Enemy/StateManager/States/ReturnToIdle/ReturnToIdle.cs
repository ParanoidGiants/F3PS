using System;
using UnityEngine;

namespace Enemy.States
{
    [Serializable]
    public class ReturnToIdle : State
    {
        private Vector3 _originalPosition;
        public override void Initialize(BaseEnemy enemy, EnemyStateManager stateManager, string name)
        {
            base.Initialize(enemy, stateManager, name);
            _originalPosition = enemy.transform.position;
            enemy.navMeshAgent.destination = _originalPosition;
        }
        
        public override void OnEnter()
        {
            base.OnEnter();
            enemy.navMeshAgent.destination = _originalPosition;
        }

        public override void Update()
        {
            if (enemy.HasReachedDestination())
            {
                stateManager.SwitchState(stateManager.idle);
                return;
            }
            
            AggressiveSwitchWhenTargetIsInSight();
        }
    }
}
