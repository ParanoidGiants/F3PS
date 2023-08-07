using System;
using UnityEngine;

namespace Enemy.States
{
    [Serializable]
    public class Aggressive : State
    {
        public Transform target;

        public void SetTarget(Transform transform)
        {
            target = transform;
        }

        public override void Update()
        {
            if (!stateManager.isTargetInSight)
            {
                stateManager.SwitchState(stateManager.checking);
                return;
            }

            enemy.navMeshAgent.destination = target.position;

            if (enemy.HasReachedDestination() && stateManager.rush.IsRecovered())
            {
                stateManager.SwitchState(stateManager.rush);
            }

            stateManager.rush.Recover();
        }
    }
}
