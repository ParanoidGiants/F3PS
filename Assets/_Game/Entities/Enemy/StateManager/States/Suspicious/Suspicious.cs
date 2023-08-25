using System;
using UnityEngine;

namespace F3PS.AI.States.Action
{
    [Serializable]
    public class Suspicious : State
    {
        private float _isSuspiciousTime;
        private float _isSuspiciousTimer = 2f;
        private Quaternion _suspiciousRotation;

        public override void OnEnter()
        {
            base.OnEnter();
            _isSuspiciousTime = _isSuspiciousTimer;
            _suspiciousRotation = enemy.transform.rotation;
        }
        
        public override void OnUpdate()
        {
            _isSuspiciousTime -= Time.deltaTime;
                
            float isSuspiciousPercenatge = _isSuspiciousTime / _isSuspiciousTimer;
            float isSuspiciousAnimateTime = Mathf.Sin(isSuspiciousPercenatge * (2f * Mathf.PI));
            enemy.transform.rotation = _suspiciousRotation * Quaternion.Euler(0, 30 * isSuspiciousAnimateTime, 0f);

            if (_isSuspiciousTime <= 0f)
            {
                _isSuspiciousTime = 0;
                stateManager.SwitchState(StateType.RETURN_TO_IDLE);
            }
        }
    }
}
