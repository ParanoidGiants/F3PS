using System;

namespace Enemy.States
{
    [Serializable]
    public class Checking : State
    {
        public override void Update()
        {
            if (stateManager.enemy.navMeshAgent.remainingDistance < 0.1f)
            {
                stateManager.SwitchState(stateManager.suspicious);
                return;
            }
            
            AggressiveSwitchWhenTargetIsInSight();
        }
    }
}
