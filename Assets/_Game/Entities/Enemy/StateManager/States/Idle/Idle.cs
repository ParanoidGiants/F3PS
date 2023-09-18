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
            _transform = enemy.transform;
        }
        
        override
        public void OnEnter()
        {
            base.OnEnter();
            _idleTime = 0f;
            navMeshAgent.isStopped = true;
        }
        
        override
        public void OnExit()
        {
            navMeshAgent.isStopped = false;
        }

        override
        public void OnUpdate()
        {
            base.OnUpdate();
            
            _idleTime += Time.deltaTime;
            if (idleTimer > _idleTime) return;
            
            stateManager.SwitchState(StateType.PATROLLING);
        }
    }
}
