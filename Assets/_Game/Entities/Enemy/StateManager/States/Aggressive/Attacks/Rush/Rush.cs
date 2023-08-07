using System;
using UnityEngine;

namespace Enemy.States
{
    [Serializable]
    public class Rush : AttackState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            enemy.Rush();
        }
        
        public override void Update()
        {
            if (enemy.Velocity.magnitude < 0.5f)
            {
                stateManager.SwitchState(stateManager.checking);
            }
        }
        
        public override void OnExit()
        {
            base.OnExit();
            enemy.StopRush();
        }
    }
}
