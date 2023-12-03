using UnityEngine;

namespace F3PS.AI.States
{
    public class Idle : State
    {
        [Space(10)]
        [Header("Specific Settings")]
        [SerializeField] private float _idleTimer = 0f;
        private float _idleTime = 0f;

        override
        public void OnEnter()
        {
            base.OnEnter();
            _idleTime = 0f;
            _navMeshAgent.isStopped = true;
        }
        
        override
        public void OnExit()
        {
            _navMeshAgent.isStopped = false;
        }

        override
        public void OnUpdate()
        {
            base.OnUpdate();
            
            _idleTime += enemy.ScaledDeltaTime;
            if (_idleTimer > _idleTime) return;
            
            stateManager.SwitchState(StateType.PATROLLING);
        }
    }
}
