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
            animator.SetFloat("Speed", 0f);
        }
        
        override
        public void OnExit()
        {
            _navMeshAgent.isStopped = false;
        }

        override
        public void OnPhysicsUpdate()
        {
            base.OnPhysicsUpdate();
            
            if (_idleTimer < 0f) return;
            
            _idleTime += enemy.ScaledDeltaTime;
            if (_idleTimer > _idleTime) return;
            
            stateManager.SwitchState(StateType.PATROLLING);
        }
    }
}
