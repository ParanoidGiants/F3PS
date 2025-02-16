using UnityEngine;

namespace F3PS.AI.States
{
    public class Dying : State
    {
        [Space(10)]
        [Header("Specific Settings")]
        public AnimationClip dieAnimation;
        private float _dieTime;

        override
        public void OnEnter()
        {
            base.OnEnter();
            _dieTime = dieAnimation.length;
            _navMeshAgent.isStopped = true;
            animator.SetTrigger("Die");
        }

        override
        public void OnPhysicsUpdate()
        {
            base.OnPhysicsUpdate();

            if (_dieTime < 0f)
            {
                return;
            }

            _dieTime -= enemy.ScaledDeltaTime;
        }
    }
}
