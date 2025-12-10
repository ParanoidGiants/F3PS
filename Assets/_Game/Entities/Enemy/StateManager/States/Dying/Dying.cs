using UnityEngine;

namespace F3PS.AI.States
{
    public class Dying : State
    {
        [Space(10)]
        [Header("Specific Settings")]
        public AnimateMesh animateMesh;
        private float _dieTime;
        private float _fadeOutTime = 1f;
        private float _fadeOutDuration = 1f;
        private bool _fadeOut = false;
        public AnimationClip dieAnimation;

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
            if (_dieTime >= 0f)
            {
                _dieTime -= enemy.ScaledDeltaTime;
                return;
            }

            if (!_fadeOut)
            {
                _fadeOut = true;
                _fadeOutTime = _fadeOutDuration;
            }

            if (_fadeOutTime >= 0f)
            {
                _fadeOutTime -= enemy.ScaledDeltaTime;
                animateMesh.FadeOut(_fadeOutTime, _fadeOutDuration);
                return;
            }
            Debug.Log("Enemy Dead");
            enemy.Died();
        }
    }
}
