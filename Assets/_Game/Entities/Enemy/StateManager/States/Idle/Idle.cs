using UnityEngine;

namespace F3PS.AI.States
{
    public class Idle : State
    {
        [Space(10)]
        [Header("Specific Settings")]
        private Transform _transform;
        
        
        public float idleTimer = 0f;
        [SerializeField] private float _idleTime = 0f;

        private void Awake()
        {
            _transform = enemy.body.transform;
        }
        
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
            if (idleTimer > _idleTime) return;
            
            stateManager.SwitchState(StateType.PATROLLING);
        }
    }
}
