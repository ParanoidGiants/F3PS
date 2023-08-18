using UnityEngine;
using UnityEngine.AI;

namespace Enemy.States
{
    public class State : MonoBehaviour
    {
        public StateType stateType;
        
        [Header("Base References")]
        public NavMeshAgent navMeshAgent;
        public BaseEnemy enemy;
        public EnemyStateManager stateManager;
        public Vision defaultVision;
        
        [Header("Base Settings")]
        public float enemySpeed;
        public float enemyStoppingDistance;
        public Material material;
        
        public virtual void OnEnter()
        {
            navMeshAgent.speed = enemySpeed;
            navMeshAgent.stoppingDistance = enemyStoppingDistance;
            enemy.SetMaterial(material);
        }

        public virtual void OnUpdate()
        {
            if (stateType != StateType.AGGRESSIVE && defaultVision.IsTargetInSight())
            {
                stateManager.SwitchState(StateType.AGGRESSIVE);
            }
        }
        public virtual void OnExit() {}
    }
}
