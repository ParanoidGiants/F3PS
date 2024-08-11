using System;
using UnityEngine;

namespace F3PS.AI.States.Action
{
    [Serializable]
    public class ReturnToIdle : State
    {
        private Vector3 _originalPosition;
        public void Awake()
        {
            _originalPosition = enemy.body.transform.position;
        }

        public override void OnEnter()
        {
            enemy.navMeshAgent.destination = _originalPosition;
        }

        public override void OnPhysicsUpdate()
        {
            base.OnPhysicsUpdate();
            if (Helper.HasReachedDestination(_navMeshAgent))
            {
                stateManager.SwitchState(StateType.IDLE);
            }
        }
    }
}
