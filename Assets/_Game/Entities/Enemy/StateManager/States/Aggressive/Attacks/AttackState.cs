using System;
using UnityEngine;

namespace Enemy.States
{
    public abstract class AttackState : State
    {
        [Space(10)]
        [Header("Base Attack Settings")]
        public float recoveryTime;
        public float recoveryTimer;
        
        public override void OnEnter()
        {
            base.OnEnter();
            recoveryTime = 0f;
        }

        public void Recover()
        {
            recoveryTime += Time.deltaTime;
        }

        public bool IsRecovered()
        {
            return recoveryTime >= recoveryTimer;
        }
    }
}
