using System;
using UnityEngine;

namespace Enemy.States
{
    [Serializable]
    public class Suspicious : State
    {
        private float isSuspiciousTime;
        public float isSuspiciousTimer;
        private Quaternion _suspiciousRotation;

        public override void OnEnter()
        {
            base.OnEnter();
            isSuspiciousTime = isSuspiciousTimer;
            _suspiciousRotation = enemy.transform.rotation;
        }
        
        public override void OnUpdate()
        {
            isSuspiciousTime -= Time.deltaTime;
                
            float isSuspiciousPercenatge = isSuspiciousTime / isSuspiciousTimer;
            float isSuspiciousAnimateTime = Mathf.Sin(isSuspiciousPercenatge * (2f * Mathf.PI));
            enemy.transform.rotation = _suspiciousRotation * Quaternion.Euler(0, 30 * isSuspiciousAnimateTime, 0f);

            if (isSuspiciousTime <= 0f)
            {
                isSuspiciousTime = 0;
                stateManager.SwitchState(StateType.RETURN_TO_IDLE);
            }
        }
    }
}
