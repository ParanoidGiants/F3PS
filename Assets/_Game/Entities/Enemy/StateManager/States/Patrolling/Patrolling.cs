using UnityEngine;

namespace F3PS.AI.States
{
    public class Patrolling : State
    {
        override
        public void OnEnter()
        {
            base.OnEnter();
            enemy.patrolManager.SetNextPatrolPoint();
            _navMeshAgent.destination = enemy.patrolManager.CurrentPatrolPoint;
        }
        
        override
        public void OnExit()
        {
            _navMeshAgent.isStopped = true;
        }

        override
        public void OnUpdate()
        {
            base.OnUpdate();
            if (!Helper.HasReachedDestination(_navMeshAgent)) return;
            
            stateManager.SwitchState(StateType.IDLE);
        }
    }
}
