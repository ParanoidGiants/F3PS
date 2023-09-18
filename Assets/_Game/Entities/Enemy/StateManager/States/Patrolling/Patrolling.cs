using UnityEngine;

namespace F3PS.AI.States
{
    public class Patrolling : State
    {
        [Space(10)]
        [Header("Specific Settings")]
        private PatrolManager _patrolManager;

        private void Start()
        {
            _patrolManager = enemy.PatrolManager;
        }
        
        override
        public void OnEnter()
        {
            base.OnEnter();
            _patrolManager.SetNextPatrolPoint();
            navMeshAgent.destination = _patrolManager.CurrentPatrolPoint;
        }
        
        override
        public void OnExit()
        {
            navMeshAgent.isStopped = true;
        }

        override
        public void OnUpdate()
        {
            base.OnUpdate();
            if (!Helper.HasReachedDestination(navMeshAgent)) return;
            
            stateManager.SwitchState(StateType.IDLE);
        }
    }
}
