namespace Enemy.States
{
    public class Checking : State
    {
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (Helper.HasReachedDestination(navMeshAgent))
            {
                stateManager.SwitchState(StateType.SUSPICIOUS);
            }
        }
    }
}
