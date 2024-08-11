using System.Numerics;
using UnityEngine;

namespace F3PS.AI.States.Action
{
    public class Checking : State
    {
        override
        public void OnEnter()
        {
            base.OnEnter();
            
            Debug.Log("ENTER CHECKING: " + enemy.name);
            _navMeshAgent.isStopped = false;
        }

        override
        public void OnPhysicsUpdate()
        {
            base.OnPhysicsUpdate();
            if (Helper.HasReachedDestination(_navMeshAgent))
            {
                stateManager.SwitchState(StateType.SUSPICIOUS);
            }
        }
    }
}
