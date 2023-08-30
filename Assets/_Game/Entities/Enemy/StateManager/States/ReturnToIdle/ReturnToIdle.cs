using System;
using UnityEngine;

namespace F3PS.AI.States.Action
{
    [Serializable]
    public class ReturnToIdle : State
    {
        private Vector3 _originalPosition;
        public void Start()
        {
            _originalPosition = enemy.transform.position;
            enemy.navMeshAgent.destination = _originalPosition;
        }
        
        public override void OnEnter()
        {
            base.OnEnter();
            enemy.navMeshAgent.destination = _originalPosition;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (Helper.HasReachedDestination(navMeshAgent))
            {
                stateManager.SwitchState(StateType.IDLE);
            }
        }
    }
}
