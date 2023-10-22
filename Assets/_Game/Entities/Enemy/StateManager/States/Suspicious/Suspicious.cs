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
        private Quaternion _suspiciousRotation;

        public override void OnEnter()
        {
            base.OnEnter();
            _isSuspiciousTime = _isSuspiciousTimer;
            _suspiciousRotation = enemy.body.transform.rotation;
        }
        
        public override void OnUpdate()
        {
            _isSuspiciousTime -= enemy.ScaledDeltaTime;
                
            float isSuspiciousPercenatge = _isSuspiciousTime / _isSuspiciousTimer;
            float isSuspiciousAnimateTime = Mathf.Sin(isSuspiciousPercenatge * (2f * Mathf.PI));
            enemy.body.transform.rotation = _suspiciousRotation * Quaternion.Euler(0, 30 * isSuspiciousAnimateTime, 0f);

            if (_isSuspiciousTime > 0f) return;
            
            stateManager.SwitchState(StateType.IDLE);
        }
    }
}
