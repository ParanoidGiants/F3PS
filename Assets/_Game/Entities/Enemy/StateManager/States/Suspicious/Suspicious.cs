using System;
using UnityEngine;

namespace F3PS.AI.States.Action
{
    [Serializable]
    public class Suspicious : State
    {
        [Header("Watchers")]
        [SerializeField] private float _isSuspiciousTime;
        [SerializeField] private float _isSuspiciousTimer = 2f;
        [SerializeField] private float _rotateAngle = 30f;
        private Quaternion _startRotation;

        public override void OnEnter()
        {
            base.OnEnter();
            _isSuspiciousTime = _isSuspiciousTimer;
            _startRotation = enemy.body.transform.rotation;
        }
        
        public override void OnPhysicsUpdate()
        {
            base.OnPhysicsUpdate();
            _isSuspiciousTime -= enemy.ScaledDeltaTime;
                
            float isSuspiciousPercenatge = _isSuspiciousTime / _isSuspiciousTimer;
            float isSuspiciousAnimateTime = Mathf.Sin(isSuspiciousPercenatge * (2f * Mathf.PI));
            enemy.body.transform.rotation = _startRotation * Quaternion.Euler(0, _rotateAngle * isSuspiciousAnimateTime, 0f);

            if (_isSuspiciousTime > 0f) return;
            
            stateManager.SwitchState(StateType.IDLE);
        }
    }
}
