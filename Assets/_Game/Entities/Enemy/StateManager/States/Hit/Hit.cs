using UnityEngine;

namespace F3PS.AI.States
{
    public class Hit : State
    {
        [Space(10)]
        [Header("Specific Settings")]
        public AnimationClip hitAnimation;
        private float _hitTime;

        override
        public void OnEnter()
        {
            base.OnEnter();
            _hitTime = hitAnimation.length;
            _navMeshAgent.isStopped = true;
            animator.SetTrigger("Hit");
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

            if (_hitTime < 0f)
            {
                stateManager.SwitchState(StateType.PATROLLING);
                return;
            }

            _hitTime -= enemy.ScaledDeltaTime;
        }
    }
}
