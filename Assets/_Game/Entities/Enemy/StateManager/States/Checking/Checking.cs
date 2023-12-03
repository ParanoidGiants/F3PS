using System.Numerics;

namespace F3PS.AI.States.Action
{
    public class Checking : State
    {
        override
        public void OnEnter()
        {
            base.OnEnter(); 
        }

        override
        public void OnUpdate()
        {
            base.OnUpdate();
            if (Helper.HasReachedDestination(_navMeshAgent))
            {
                stateManager.SwitchState(StateType.SUSPICIOUS);
            }
        }
    }
}
