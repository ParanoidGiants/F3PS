using UnityEngine;

namespace Enemy.States
{
    public class Idle : State
    {
        private Transform _transform;
        private Quaternion _originalRotation;
        private Quaternion _startRotation;
        private float _rotateTime;
        private readonly float _rotateTimer = 1f;

        private void Awake()
        {
            _transform = enemy.transform;
            _originalRotation = _transform.rotation;
        }
        
        override
        public void OnEnter()
        {
            base.OnEnter();
            _startRotation = _transform.rotation;
            _rotateTime = 0f;
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
            
            if (_rotateTime >= _rotateTimer) return;
            
            _transform.rotation = Quaternion.Slerp(_startRotation, _originalRotation, _rotateTime);
            _rotateTime += Time.deltaTime;
        }
    }
}
