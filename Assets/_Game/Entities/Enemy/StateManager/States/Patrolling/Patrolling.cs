using UnityEngine;

namespace F3PS.AI.States
{
    public class Patrolling : State
    {
        override
        public void OnEnter()
        {
            base.OnEnter(); 
            _navMeshAgent.isStopped = false;
            enemy.patrolManager.SetNextPatrolPoint();
            _navMeshAgent.destination = enemy.patrolManager.CurrentPatrolPoint;
        }
        
        override
        public void OnExit()
        {
            
            Debug.Log("EXIT PATROL: " + enemy.name);
            _navMeshAgent.isStopped = true;
        }

        override
        public void OnPhysicsUpdate()
        {
            base.OnPhysicsUpdate();
            if (!Helper.HasReachedDestination(_navMeshAgent)) return;
            
            stateManager.SwitchState(StateType.IDLE);
        }
    }
}
